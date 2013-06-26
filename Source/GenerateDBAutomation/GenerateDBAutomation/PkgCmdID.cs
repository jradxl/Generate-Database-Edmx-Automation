// PkgCmdID.cs
// MUST match PkgCmdID.h

namespace Company.VSPackageCM
{
    static class PkgCmdIDList
    {
        public const uint cmdidRefreshContextMenu = 0x0103;

        public const uint cmdidMyTool = 0x2001;
        public const int cmdidClearWindowsList = 0x2002;
        public const int cmdidRefreshWindowsList = 0x2003;

        // Define the list of menus (these include toolbars)
        public const int IDM_MyToolbar = 0x0101;
    };
}