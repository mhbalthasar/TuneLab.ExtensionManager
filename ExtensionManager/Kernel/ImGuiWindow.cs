using System.Numerics;
using ImGuiNET;
using ImGuiNET.SDL2CS;

namespace ExtensionManager
{
    public class ImGuiWindow : ImWindowExt
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

        static ImGuiWindowFlags win_flags = ImGuiWindowFlags.None;
        static int win_width = 640;
        static int win_height = 320;


        public ImGuiWindow() : base(Mode.Single, "", null, win_width, win_height, win_flags)
        {
            Title = Worker.GuiTitle;
            BackgroundColor = new Vector4(0, 0, 0, 0);
            Worker.UpdateExtensions();
            mAction = SubmitUI;
        }

        bool SubmitUI()
        {
            if (_showAlertWindow)
            {
                ImGui.SetNextWindowSize(new Vector2(this.Size.X / 2, this.Size.Y / 2), ImGuiCond.Always);
                ImGui.SetNextWindowPos(new Vector2(this.Size.X / 4, this.Size.Y / 4), ImGuiCond.Always);
                ImGui.Begin("Alert", ref _showAlertWindow, ImGuiWindowFlags.NoCollapse & ImGuiWindowFlags.AlwaysAutoResize);
                ImGui.TextWrapped(_textPopupWindow);
                ImGui.SetCursorPos(new Vector2(0, this.Size.Y / 2 - 40));
                ImGui.Separator();
                ImGui.SetCursorPos(new Vector2(this.Size.X / 4 - 50, this.Size.Y / 2 - 35));
                if (ImGui.Button("OK", new Vector2(100, 25)))
                    _showAlertWindow = false;
                ImGui.End();
            }
            else if (_showConfirmWindow)
            {
                ImGui.SetNextWindowSize(new Vector2(this.Size.X / 2, this.Size.Y / 2), ImGuiCond.Always);
                ImGui.SetNextWindowPos(new Vector2(this.Size.X / 4, this.Size.Y / 4), ImGuiCond.Always);
                ImGui.Begin("Confirm", ref _showAlertWindow, ImGuiWindowFlags.NoCollapse & ImGuiWindowFlags.AlwaysAutoResize);
                ImGui.TextWrapped(_textPopupWindow);
                ImGui.SetCursorPos(new Vector2(0, this.Size.Y / 2 - 40));
                ImGui.Separator();
                ImGui.SetCursorPos(new Vector2(this.Size.X / 8 - 50, this.Size.Y / 2 - 35));
                if (ImGui.Button("Yes", new Vector2(100, 25)))
                {
                    _actionConfirmWindow(_objectConfirmWindow);
                    _showConfirmWindow = false;
                }
                ImGui.SetCursorPos(new Vector2((this.Size.X * 3 / 8) - 50, this.Size.Y / 2 - 35));
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
                ImGui.SetCursorPos(new Vector2(10, 15));
                ImGui.Text("Extensions:");
                ImGui.SetCursorPos(new Vector2(10, 40));
                ImGui.PushItemWidth(500);
                if (ImGui.ListBox("", ref Worker.ExtensionSelectedIndex, Worker.ExtensionList.ToArray(), Worker.ExtensionList.Count, 14))
                {
                    // Alert(Worker.ExtensionSelectedIndex.ToString());
                };
                ImGui.PopItemWidth();

                ImGui.SetCursorPos(new Vector2(525, 40));
                if (ImGui.Button("Install", new Vector2(100, 35)))
                {
                    if (Worker.CheckTuneLabAlive())
                        Alert("TuneLab is Running!");
                    else
                        Worker.InstallExtension();
                }
                ImGui.SetCursorPos(new Vector2(525, 80));
                if (ImGui.Button("Uninstall", new Vector2(100, 35)))
                {
                    if (Worker.CheckTuneLabAlive())
                        Alert("TuneLab is Running!");
                    else if (Worker.ExtensionSelectedIndex >= 0 && Worker.ExtensionList.Count > Worker.ExtensionSelectedIndex)
                        Confirm(String.Format("Did you confirm to Uninstall the extision:{0} ?", Worker.ExtensionList[Worker.ExtensionSelectedIndex]), Worker.UninstallExtension, Worker.ExtensionSelectedIndex);
                }

                if (Worker.IsHaveSetting(Worker.ExtensionSelectedIndex))
                {
                    ImGui.SetCursorPos(new Vector2(525, 120));
                    if (ImGui.Button("Setting", new Vector2(100, 35)))
                    {
                        if (Worker.ExtensionSelectedIndex >= 0 && Worker.ExtensionList.Count > Worker.ExtensionSelectedIndex)
                            Worker.StartSetting(Worker.ExtensionSelectedIndex);
                    }
                }
                ImGui.SetCursorPos(new Vector2(525, 160));
                if (Worker.CheckTuneLabAlive()) if (ImGui.Button("Unlock\nTunelab", new Vector2(100, 35)))
                    {
                        if (Worker.CheckTuneLabAlive())
                            Worker.UnlockTuneLabAlive();
                    }
            }
            return true;
        }
    }
}