using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.Native;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace EMS_PJT_Hamburger.Models.Managers
{
    public class ValueChangeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isUp = (bool)value;
            return isUp ? Brushes.White : Brushes.DarkGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
    public class ConvertManager
    {
        public ImageSource GetSvgImage(string imagePath, Size imageSize)
        {
            var extension = new SvgImageSourceExtension() { Uri = new Uri(imagePath), Size = imageSize };
            return (ImageSource)extension.ProvideValue(null);
        }

        private ulong ToUInt64LittleEndian(byte[] data)
        {
            ulong value = 0;
            for (int i = 0; i < data.Length; i++)
            {
                value |= ((ulong)data[i]) << (8 * i);
            }
            return value;
        }
    }
    public class ReadyConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
        {
            if (value is int i && i == 0)
            {
                return "Not Ready";
            }
            if (value is double d && d == 0)
            {
                return "Not Ready";
            }
            return "Ready";
        }
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
    }

    public class RelayConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
        {
            if (value is bool b && !b)
            {
                return "OFF";
            }
            return "ON";
        }
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
    }
    public class ControlConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
        {
            if (value is int i && i == 0)
            {
                return "CC+CV";
            }
            return "CP";
        }
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
    }
    public class OpenConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
        {
            if (value is int i && i == 0)
            {
                return "Close";
            }
            if (value is double d && d == 0)
            {
                return "Close";
            }
            return "Open";
        }
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
    }
    public class ValueToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int i)
            {
                if (i == 0)
                    return Brushes.Gray;
            }
            
                    return Brushes.Lime;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
                => throw new NotImplementedException();
    }
    public class BoxToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
            {
                return Brushes.Lime;
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class RiskToSvgConverter : IValueConverter
    {
        private static readonly ImageSource LowPriority = Create(
            "pack://application:,,,/DevExpress.Images.v23.1;component/SvgImages/Outlook Inspired/LowPriority.svg");

        private static readonly ImageSource NormalPriority = Create(
            "pack://application:,,,/DevExpress.Images.v23.1;component/SvgImages/Outlook Inspired/NormalPriority.svg");

        private static readonly ImageSource MediumPriority = Create(
            "pack://application:,,,/DevExpress.Images.v23.1;component/SvgImages/Outlook Inspired/MediumPriority.svg");

        private static readonly ImageSource HighPriority = Create(
            "pack://application:,,,/DevExpress.Images.v23.1;component/SvgImages/Outlook Inspired/HighPriority.svg");
        private static ImageSource Create(string packUri)
        {
            return WpfSvgRenderer.CreateImageSource(new Uri(packUri, UriKind.Absolute));
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return LowPriority;

            int risk = 0;
            if (value is int i)
                risk = i;
            
            switch (risk)
            {
                case 0: return LowPriority;
                case 1: return NormalPriority;
                case 2: return MediumPriority;
                default: return HighPriority;
            }
        }
        

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
    
}
