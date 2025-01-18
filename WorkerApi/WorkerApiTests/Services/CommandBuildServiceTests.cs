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

            Assert.AreEqual(cmd.Args.ToString(), " -i \"$AMIYA_IN_0\" -filter_complex \"$AMIYA_FC\" -map \"[$AMIYA_OUTVID_0]\" -map \"[$AMIYA_OUTAUD_0]\" -f flv \"$AMIYA_OUT_0\"");
            Assert.AreEqual(cmd.Env.Count, 5);
            Assert.AreEqual(cmd.Env["AMIYA_FC"], "[0:v]null[s0];[0:a]anull[s1];[s0]drawbox=x=10:y=5:w=iw-10-x:h=ih-0-y:color=#FFFFFF:t=5[s2];[s2]hflip,vflip[s3]");
            Assert.AreEqual(cmd.Env["AMIYA_IN_0"], "rtmp://localhost/mystream1");
            Assert.AreEqual(cmd.Env["AMIYA_OUT_0"], "rtmp://localhost/mystream2");
        }

        [TestMethod()]
        public void BuildCommand2Test()
        {
            VideoCommand cmd = _service.BuildCommand(ReadJson("command2.json"));

            Assert.AreEqual(cmd.Args.ToString(), " -i \"$AMIYA_IN_0\" -i \"$AMIYA_IN_1\" -filter_complex \"$AMIYA_FC\" -map \"[$AMIYA_OUTVID_0]\" -map \"[$AMIYA_OUTAUD_0]\" -f flv \"$AMIYA_OUT_0\"");
            Assert.AreEqual(cmd.Env.Count, 6);
            Assert.AreEqual(cmd.Env["AMIYA_FC"], "[0]null[s0];[1]anull[s1];[s0]split[s2][s3];[s3]split[s3:0][s3:1];[s2][s3:0]scale=-1:rh,[s3:1]hstack[s4]");
            Assert.AreEqual(cmd.Env["AMIYA_IN_1"], "rtmp://localhost/mystream2");
            Assert.AreEqual(cmd.Env["AMIYA_OUTAUD_0"], "s1");
            Assert.AreEqual(cmd.Env["AMIYA_OUTVID_0"], "s4");
        }

        [TestMethod()]
        public void BuildCommand3Test()
        {
            VideoCommand cmd = _service.BuildCommand(ReadJson("command3.json"));
            string compiled = cmd.Compile();
            Assert.AreEqual(compiled,
                " -i \"rtmp://localhost/mystream1\" -filter_complex \"[0]null[s0];[s0]split[s0:1],crop=w=iw/1.5:h=ih/1:x=(iw-ow)/2+0:y=(ih-oh)/2-50[s0:0];[s0:0][s0:1]scale=rw:rh[s1];[s1]drawtext=x=w/2:y=0.8*h:fontcolor=#FF0000:text='AMOGUS\\\\:':fontsize=20[s2];[s2]split[s2:1],crop=w=iw/1.5:h=ih/1.5:x=(iw-ow)/2+-75:y=(ih-oh)/2-0[s2:0];[s2:0][s2:1]scale=rw:rh[s3];[s3]drawtext=x=0:y=0.5*h:fontcolor=#000000:text=''\\\\\\\\\\\\''caractère_ spécial\\\"':fontsize=16[s4]\" -map \"[s4]\" -f flv \"rtmp://localhost/mystream2\""
                );
        }
    }
}