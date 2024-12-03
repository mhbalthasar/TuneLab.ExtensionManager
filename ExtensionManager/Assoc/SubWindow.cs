using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using ImGuiNET.SDL2CS;

namespace AssocManager
{
    public class SubWindow
    {
        private bool _showAlertWindow = false;
        private string _textPopupWindow = "";
        private bool _showConfirmWindow = false;
        private Action<object> _actionConfirmWindow = null;
        private object _objectConfirmWindow = null;

        private void Alert(string content)
        {
            _textPopupWindow = content;
            _showAlertWindow = true;
        }
        private void Confirm(string content, Action<object> action, object obj)
        {
            _textPopupWindow = content;
            _showConfirmWindow = true;
            _actionConfirmWindow = action;
            _objectConfirmWindow = obj;
        }

        Vector2 baseSize;

        public SubWindow(Vector2 baseSize)
        {
            this.baseSize = baseSize;
            Worker.LoadPath();
            Worker.UpdateInfo();
        }

        bool _isOpen = false;
        public bool IsOpen()
        {
            return _isOpen;
        }
        public void Open() => _isOpen = true;
        public void Close() => _isOpen = false;
        public bool SubmitUI()
        {
            if (_showAlertWindow)
            {
                ImGui.SetNextWindowSize(new Vector2(baseSize.X / 2, baseSize.Y / 2), ImGuiCond.Always);
                ImGui.SetNextWindowPos(new Vector2(baseSize.X / 4, baseSize.Y / 4), ImGuiCond.Always);
                ImGui.Begin("Alert", ref _showAlertWindow, ImGuiWindowFlags.NoCollapse & ImGuiWindowFlags.AlwaysAutoResize);
                ImGui.TextWrapped(_textPopupWindow);
                ImGui.SetCursorPos(new Vector2(0, baseSize.Y / 2 - 40));
                ImGui.Separator();
                ImGui.SetCursorPos(new Vector2(baseSize.X / 4 - 50, baseSize.Y / 2 - 35));
                if (ImGui.Button("OK", new Vector2(100, 25)))
                    _showAlertWindow = false;
                ImGui.End();
            }
            else if (_showConfirmWindow)
            {
                ImGui.SetNextWindowSize(new Vector2(baseSize.X / 2, baseSize.Y / 2), ImGuiCond.Always);
                ImGui.SetNextWindowPos(new Vector2(baseSize.X / 4, baseSize.Y / 4), ImGuiCond.Always);
                ImGui.Begin("Confirm", ref _showAlertWindow, ImGuiWindowFlags.NoCollapse & ImGuiWindowFlags.AlwaysAutoResize);
                ImGui.TextWrapped(_textPopupWindow);
                ImGui.SetCursorPos(new Vector2(0, baseSize.Y / 2 - 40));
                ImGui.Separator();
                ImGui.SetCursorPos(new Vector2(baseSize.X / 8 - 50, baseSize.Y / 2 - 35));
                if (ImGui.Button("Yes", new Vector2(100, 25)))
                {
                    _actionConfirmWindow(_objectConfirmWindow);
                    _showConfirmWindow = false;
                }
                ImGui.SetCursorPos(new Vector2((baseSize.X * 3 / 8) - 50, baseSize.Y / 2 - 35));
                if (ImGui.Button("No", new Vector2(100, 25)))
                    _showConfirmWindow = false;
                ImGui.End();
            }
            else if (Worker.isWorking)
            {
                ImGui.SetCursorPos(new Vector2(10, 15));
                ImGui.Text("Program is running for your action, please wait for a moment.");
            }
            else
            {
                ImGui.SetCursorPos(new Vector2(10, 25));
                ImGui.Text("TuneLab Path:");

                ImGui.SetCursorPos(new Vector2(100, 23));
                ImGui.PushItemWidth(465);
                ImGui.InputText("  ", ref Worker.TuneLabPath, (uint)65535);
                ImGui.PopItemWidth();

                ImGui.SetCursorPos(new Vector2(575, 23));
                if (ImGui.Button("...", new Vector2(50, 18)))
                {
                    var ret = NativeFileDialogSharp.Dialog.FolderPicker(Worker.TuneLabPath);
                    if (ret.IsOk)
                    {
                        string ext = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))?".exe":"";
                        if (
                            File.Exists(Path.Combine(ret.Path, "TuneLab" + ext)) &&
                            File.Exists(Path.Combine(ret.Path, "ExtensionInstaller" + ext))
                            )
                        {
                            Worker.TuneLabPath = ret.Path;
                            Worker.SavePath();
                            Worker.UpdateInfo();
                        }
                    }
                }
                {
                    ImGui.SetCursorPos(new Vector2(10, 60));
                    ImGui.Text(String.Format("TuneLab Version: {0}",Worker.Version));
                }
                {
                    ImGui.GetWindowDrawList().AddLine(
                        new Vector2(10, 90),
                        new Vector2(625, 90),
                        ImGui.GetColorU32(new Vector4(1, 1, 1, 1)),
                        1.0f                                     // 线宽
                    );
                }
                {
                    ImGui.SetCursorPos(new Vector2(50, 150));
                    if (ImGui.Button("Create\nDesktop Icon", new Vector2(100, 80)))
                    {
                        if(Worker.CreateIcon(1))Alert("Created Desktop Icon!");
                    }

                    ImGui.SetCursorPos(new Vector2(200, 150));
                    if (ImGui.Button("Create\nStartMenu Icon", new Vector2(100, 80)))
                    {
                        if (Worker.CreateIcon(2)) Alert("Created StartMenu Icon!");
                    }

                    ImGui.SetCursorPos(new Vector2(350, 150));
                    if (ImGui.Button("Assoc\nFile Format", new Vector2(100, 80)))
                    {
                        if (!Worker.SupportCmdOpen) Alert("This Function Only Support TuneLab Version Above 1.5.9");
                        else if (Worker.SetAlloc(0)) Alert("Done!");
                    }


                    ImGui.SetCursorPos(new Vector2(525, 200));
                    if (ImGui.Button("<<Back", new Vector2(100, 35)))
                    {
                        Close();
                    }
                }
            }
            return true;
        }
    }
}