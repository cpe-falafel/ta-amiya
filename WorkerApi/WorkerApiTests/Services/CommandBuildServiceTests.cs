using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WorkerApi.Models;

namespace WorkerApi.Services.Tests
{
    [TestClass()]
    public class CommandBuildServiceTests
    {
        private ICommandBuildService _service;

        private String ReadJson(String filename)
        {
            string folderPath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "Resources"), "CommandBuildService");
            return File.ReadAllText(Path.Combine(folderPath, filename));
        }

        private string SerializeCmd(VideoCommand cmd)
        {
            return String.Join("__", cmd.Args.ToArray());
        }

        public CommandBuildServiceTests() {
            var services = new ServiceCollection();
            services.AddLogging(configure => { });
            services.AddTransient<ICommandBuildService, CommandBuildService>();
            services.AddTransient<IFilterGraphService, FilterGraphService>();
            var provider = services.BuildServiceProvider();
            _service = provider.GetRequiredService<ICommandBuildService>();
        }


        [TestMethod()]
        public void BuildCommand1Test()
        {
            VideoCommand cmd = _service.BuildCommand(ReadJson("command1.json"));

            string expected = "-i__rtmp://localhost/mystream1__-filter_complex__" +
                "[0:v]null[s0];[0:a]anull[s1];[s0]drawbox=x=10:y=5:w=iw-10-x:h=ih-0-y:color=#FFFFFF:t=5[s2];[s2]hflip,vflip[s3]" +
                "__-map__[s3]__-map__[s1]__-f__flv__rtmp://localhost/mystream2";

            Assert.AreEqual(SerializeCmd(cmd), expected);
        }

        [TestMethod()]
        public void BuildCommand2Test()
        {
            VideoCommand cmd = _service.BuildCommand(ReadJson("command2.json"));

            string expected = "-i__rtmp://localhost/mystream1__-i__rtmp://localhost/mystream2__-filter_complex__" +
                "[0]null[s0];[1]anull[s1];[s0]split[s2][s3];[s3]split[s3:0][s3:1];[s2][s3:0]scale=-1:rh,[s3:1]hstack[s4]" +
                "__-map__[s4]__-map__[s1]__-f__flv__rtmp://localhost/mystream3";

            Assert.AreEqual(SerializeCmd(cmd), expected);
        }

        [TestMethod()]
        public void BuildCommand3Test()
        {

            VideoCommand cmd = _service.BuildCommand(ReadJson("command3.json"));

            string expected = "-i__rtmp://localhost/mystream1__-filter_complex__"+
                "[0]null[s0];[s0]split[s0:1],crop=w=iw/1.5:h=ih/1:x=(iw-ow)/2+0:y=(ih-oh)/2-50[s0:0];[s0:0][s0:1]scale=rw:rh[s1];[s1]drawtext=x=w/2:y=0.8*h:fontcolor=#FF0000:text='AMOGUS\\:':fontsize=20[s2];[s2]split[s2:1],crop=w=iw/1.5:h=ih/1.5:x=(iw-ow)/2+-75:y=(ih-oh)/2-0[s2:0];[s2:0][s2:1]scale=rw:rh[s3];[s3]drawtext=x=0:y=0.5*h:fontcolor=#000000:text=''\\\\\\''caractère_ spécial\"':fontsize=16[s4]" +
                "__-map__[s4]__-f__flv__rtmp://localhost/mystream2";

            Assert.AreEqual(SerializeCmd(cmd), expected);
        }
    }
}