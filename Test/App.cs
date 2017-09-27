using System;
using XL.Runtime;

namespace Test {
    public class App {
        private static void Main(string[] args) {
            Scheduler scheduler = new Scheduler();
            scheduler.Fork(new test._type305(new test._type304()), new ProcessHandler(test._main._action18));
            scheduler.Run();
        }
    }
}
