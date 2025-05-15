using System.Windows.Media;

namespace FileDeduplicator.Services
{
    public class BackgroundColorServices
    {
        private int _index = 0;
        private readonly byte _step = 23;
        private byte _accumulatedStep = 0;

        public Brush GetNextColor()
        {
            // 当索引回到 0 时，增加累积步长 (byte 会自动回绕)
            if (_index == 0)
                _accumulatedStep += _step;

            // 初始化 RGB 分量为白色
            byte[] rgb = { 255, 255, 255 };

            // 根据当前索引减少相应的颜色分量 (byte 会自动回绕)
            rgb[_index] -= _accumulatedStep;

            // 更新索引，循环0~2
            _index = (_index + 1) % 3;

            return new SolidColorBrush(Color.FromRgb(rgb[0], rgb[1], rgb[2]));
        }
    }
}
