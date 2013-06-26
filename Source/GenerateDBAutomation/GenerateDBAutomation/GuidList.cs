
namespace Company.VSPackageCM
{
    using System;

    static class GuidList
    {
        public const string guidVSPackageGDBAPkgString = "1f760350-56cc-4ecf-9e80-d89675cd8455";
        public const string guidVSPackageGDBACmdSetString = "fdd58a6e-5a92-4237-967e-d5d4eb3abcac";

        public const string guidRdtEventExplorerCmdSetString = "f520383c-4ee3-4155-a499-2fe423f5e9e6";
        public const string guidToolWindowPersistanceString = "99cd759f-e9ab-4327-985a-040573ac417a";


        public static readonly Guid guidVSPackageGDBACmdSet = new Guid(guidVSPackageGDBACmdSetString);

        public static readonly Guid guidRdtEventExplorerCmdSet = new Guid(guidRdtEventExplorerCmdSetString);
        public static readonly Guid guidToolWindowPersistance = new Guid(guidToolWindowPersistanceString);
    };
}
