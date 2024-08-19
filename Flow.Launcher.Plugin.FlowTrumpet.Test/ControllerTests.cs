using Flow.Launcher.Plugin.FlowTrumpet.Audio;
using Moq;

namespace Flow.Launcher.Plugin.FlowTrumpet.Tests
{
    public class ControllerTests
    {
        private Controller _controller;

        private Mock<IPublicAPI> _publicApiMock;
        private Mock<IAudioSessionManager> _audioManagerMock;

        [SetUp]
        public void Setup()
        {
            _publicApiMock = new Mock<IPublicAPI>();
            var pluginMetadata = new PluginMetadata()
            {
                ActionKeyword = "vol",
            };
            _audioManagerMock = new Mock<IAudioSessionManager>();

            _controller = new Controller(_publicApiMock.Object, pluginMetadata, _audioManagerMock.Object);
        }

        [Test]
        public void GetDefaultSessionList_ListSessions_Works()
        {
            // setup
            var sessionsInfo = new List<AudioSessionInfo>()
            {
                new AudioSessionInfo
                {
                    Name = "Browser",
                    ProcessId = 123,
                    Volume = 0.5f,
                },
                new AudioSessionInfo
                {
                    Name = "Music Player",
                    ProcessId = 54,
                    Volume = 0.23f,
                }
            };

            _audioManagerMock.Setup(p => p.GetSessionsInfo()).Returns(sessionsInfo);

            // execute
            var sessionList = _controller.GetDefaultSessionList().ToList();

            // verify
            Assert.IsNotNull(sessionList);
            Assert.That(sessionList.Count, Is.EqualTo(2));

            for (int i = 0; i < 2; i++)
            {
                Assert.That(sessionList[i].Title, Is.EqualTo($"{sessionsInfo[i].Name} - {(uint)(sessionsInfo[i].Volume * 100)}%"));
                Assert.That(sessionList[i].SubTitle, Is.EqualTo($"Id: {sessionsInfo[i].ProcessId}"));
            }

            _audioManagerMock.Verify(p => p.GetSessionsInfo());
        }

        [Test]
        public void GetDefaultSessionList_ListSessions_Empty_GiveInfo()
        {
            // setup
            var sessionsInfo = new List<AudioSessionInfo>();
            _audioManagerMock.Setup(p => p.GetSessionsInfo()).Returns(sessionsInfo);

            // execute
            var sessionList = _controller.GetDefaultSessionList().ToList();

            // verify
            Assert.IsNotNull(sessionList);
            Assert.That(sessionList.Count, Is.EqualTo(1));
            Assert.That(sessionList[0].Title, Is.EqualTo("No active application"));

            _audioManagerMock.Verify(p => p.GetSessionsInfo());
        }

        [Test]
        public void GetSessionOptions_WrongProcessId_ReturnEmpty()
        {
            // setup
            _audioManagerMock.Setup(p => p.GetSessionInfo(It.IsAny<uint>())).Returns<AudioSessionInfo?>(null);
            var actionMock = new Mock<Action<AudioSessionInfo>>();

            // execute
            var result = _controller.GetSessionOptions(123, actionMock.Object);

            // verify
            Assert.IsNotNull(result);
            Assert.That(result.Count(), Is.EqualTo(0));
            actionMock.Verify(p => p.Invoke(It.IsAny<AudioSessionInfo>()), Times.Never());
        }

        [Test]
        public void GetSessionOptions_ExecuteAction_Works()
        {
            // setup
            var audioSessionInfo = new AudioSessionInfo
            {
                Name = "Music Player",
                ProcessId = 2137,
                Volume = 1.0f,
            };

            _audioManagerMock.Setup(p => p.GetSessionInfo(It.IsAny<uint>())).Returns(audioSessionInfo);
            var actionMock = new Mock<Action<AudioSessionInfo>>();

            // execute
            var results = _controller.GetSessionOptions(123, actionMock.Object).ToList();
            Assert.IsNotNull(results);

            results.ElementAt(0).Action(new ActionContext());

            // verify
            actionMock.Verify(p => p.Invoke(audioSessionInfo));

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].Title, Is.EqualTo($"{audioSessionInfo.Name} - {(uint)(audioSessionInfo.Volume * 100)}%"));
            Assert.That(results[0].SubTitle, Is.EqualTo("Type number to set new volume"));
        }

        [Test]
        public void ProcessQuery_EmptySearch_ReturnDefaultSessionList()
        {
            // setup
            var sessionsInfo = new List<AudioSessionInfo>()
            {
                new AudioSessionInfo
                {
                    Name = "Browser",
                    ProcessId = 123,
                    Volume = 0.5f,
                },
                new AudioSessionInfo
                {
                    Name = "Music Player",
                    ProcessId = 54,
                    Volume = 0.23f,
                }
            };

            _audioManagerMock.Setup(p => p.GetSessionsInfo()).Returns(sessionsInfo);

            // execute
            var results = _controller.ProcessQuery("", "").ToList();

            // verify
            Assert.IsNotNull(results);
            Assert.That(results.Count, Is.EqualTo(2));

            for (int i = 0; i < 2; i++)
            {
                Assert.That(results[i].Title, Is.EqualTo($"{sessionsInfo[i].Name} - {(uint)(sessionsInfo[i].Volume * 100)}%"));
                Assert.That(results[i].SubTitle, Is.EqualTo($"Id: {sessionsInfo[i].ProcessId}"));
            }

            _audioManagerMock.Verify(p => p.GetSessionsInfo());
        }

        [Test]
        public void ProcessQuery_SearchOne_Works()
        {
            // setup
            var sessionsInfo = new List<AudioSessionInfo>()
            {
                new AudioSessionInfo
                {
                    Name = "Browser",
                    ProcessId = 123,
                    Volume = 0.5f,
                },
                new AudioSessionInfo
                {
                    Name = "Music Player",
                    ProcessId = 54,
                    Volume = 0.23f,
                }
            };

            _audioManagerMock.Setup(p => p.GetSessionsInfo()).Returns(sessionsInfo);
            _publicApiMock.Setup(p => p.ChangeQuery(It.IsAny<string>(), It.IsAny<bool>()));

            // execute
            var results = _controller.ProcessQuery("Mus", "").ToList();
            Assert.IsNotNull(results);
            Assert.That(results.Count, Is.EqualTo(1));

            results[0].Action(new ActionContext());

            // verify
            Assert.That(results[0].Title, Is.EqualTo($"{sessionsInfo[1].Name} - {(uint)(sessionsInfo[1].Volume * 100)}%"));
            Assert.That(results[0].SubTitle, Is.EqualTo($"Id: {sessionsInfo[1].ProcessId}"));

            _audioManagerMock.Verify(p => p.GetSessionsInfo());
            _publicApiMock.Verify(p => p.ChangeQuery("vol 54 ", false));
        }

        [Test]
        public void ProcessQuery_InvalidSearch_ReturnInfo()
        {
            // setup
            var sessionsInfo = new List<AudioSessionInfo>()
            {
                new AudioSessionInfo
                {
                    Name = "Browser",
                    ProcessId = 123,
                    Volume = 0.5f,
                },
                new AudioSessionInfo
                {
                    Name = "Music Player",
                    ProcessId = 54,
                    Volume = 0.23f,
                }
            };

            _audioManagerMock.Setup(p => p.GetSessionsInfo()).Returns(sessionsInfo);

            // execute
            var results = _controller.ProcessQuery("zxcfd", "").ToList();

            // verify
            Assert.IsNotNull(results);
            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].Title, Is.EqualTo("Process not found"));

            _audioManagerMock.Verify(p => p.GetSessionsInfo());
        }

        [Test]
        public void ProcessQuery_GetProcessOptions_Works()
        {
            // setup
            var sessionInfo = new AudioSessionInfo
            {
                Name = "Music Player",
                ProcessId = 54,
                Volume = 0.23f,
            };

            _audioManagerMock.Setup(p => p.GetSessionInfo(It.IsAny<uint>())).Returns(sessionInfo);

            // execute
            var results = _controller.ProcessQuery("54", "").ToList();

            // verify
            Assert.IsNotNull(results);
            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].Title, Is.EqualTo($"{sessionInfo.Name} - {(uint)(sessionInfo.Volume * 100)}%"));
            Assert.That(results[0].SubTitle, Is.EqualTo("Type number to set new volume"));

            _audioManagerMock.Verify(p => p.GetSessionInfo(54));
        }

        [Test]
        public void ProcessQuery_GetProcessOptions_InvalidProcesdId_ReturnInfo()
        {
            // setup
            var sessionsInfo = new List<AudioSessionInfo>()
            {
                new AudioSessionInfo
                {
                    Name = "Browser",
                    ProcessId = 123,
                    Volume = 0.5f,
                },
                new AudioSessionInfo
                {
                    Name = "Music Player",
                    ProcessId = 54,
                    Volume = 0.23f,
                }
            };

            _audioManagerMock.Setup(p => p.GetSessionsInfo()).Returns(sessionsInfo);
            _audioManagerMock.Setup(p => p.GetSessionInfo(It.IsAny<uint>())).Returns<AudioSessionInfo?>(null);

            // execute
            var results = _controller.ProcessQuery("542", "").ToList();

            // verify
            Assert.IsNotNull(results);
            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].Title, Is.EqualTo("Process not found"));

            _audioManagerMock.Verify(p => p.GetSessionInfo(542));
            _audioManagerMock.Verify(p => p.GetSessionsInfo());
        }

        [Test]
        public void ProcessQuery_GetProcessOptions_SetVolume_Works()
        {
            // setup
            var sessionInfo = new AudioSessionInfo
            {
                Name = "Music Player",
                ProcessId = 54,
                Volume = 0.23f,
            };

            _audioManagerMock.Setup(p => p.SetSessionVolume(It.IsAny<uint>(), It.IsAny<float>()));
            _audioManagerMock.Setup(p => p.GetSessionInfo(It.IsAny<uint>())).Returns(sessionInfo);

            // execute
            var results = _controller.ProcessQuery("54", "50").ToList();

            // verify
            Assert.IsNotNull(results);
            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].Title, Is.EqualTo($"{sessionInfo.Name} - {(uint)(sessionInfo.Volume * 100)}%"));
            Assert.That(results[0].SubTitle, Is.EqualTo("Type number to set new volume"));

            _audioManagerMock.Verify(p => p.GetSessionInfo(54));

            // execute
            results[0].Action(new ActionContext());

            // verify
            _audioManagerMock.Verify(p => p.SetSessionVolume(54, 0.5f));
        }
    }
}
