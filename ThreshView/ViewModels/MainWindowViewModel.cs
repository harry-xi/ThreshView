using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using ReactiveUI;
using ThreshView.Services;

namespace ThreshView.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IImageLoader _loader;
    private readonly IImageProcessing _processing;

    public MainWindowViewModel() : this(new ImageLoader(), new ImageProcessing())
    {
    }

    private MainWindowViewModel(IImageLoader loader, IImageProcessing processing)
    {
        _loader = loader;
        _processing = processing;
        OpenImages = new ObservableCollection<ImageDocumentViewModel>();
        OpenCommand = ReactiveCommand.CreateFromTask(OpenFilesAsync);
    }

    public ObservableCollection<ImageDocumentViewModel> OpenImages { get; }

    public ImageDocumentViewModel? SelectedImage
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public Color ThresholdColor
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = new(100, 255, 0, 0);

    public int Threshold
    {
        get;
        set =>
            this.RaiseAndSetIfChanged(ref field, value);
    } = 100;

    // Expose as ICommand so XAML analyzers recognize it
    public ICommand OpenCommand { get; }

    public IColorPalette ColorPalette { get; } = new ThresholdColorPaletteIColorPalette();

    private async Task OpenFilesAsync()
    {
        var topLevel = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;
        if (topLevel == null) return;
        var storageProvider = TopLevel.GetTopLevel(topLevel)?.StorageProvider;
        if (storageProvider == null) return;
        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = true
        });
        if (files.Count == 0) return;

        foreach (var file in files)
        {
            var path = file.Path.LocalPath;
            var model = await _loader.LoadImageAsync(path);
            var vm = new ImageDocumentViewModel(model, _processing, this);
            OpenImages.Add(vm);
            SelectedImage = vm;
        }
    }

    private class ThresholdColorPaletteIColorPalette : IColorPalette
    {
        private static readonly Color[,] Colors = new[,]
        {
            {
                //Red
                Color.FromUInt32(0x80FEF2ED),
                Color.FromUInt32(0x80FEDDD2),
                Color.FromUInt32(0x80FDB7A5),
                Color.FromUInt32(0x80FB9078),
                Color.FromUInt32(0x80FA664C),
                Color.FromUInt32(0x80F93920),
                Color.FromUInt32(0x80D52515),
                Color.FromUInt32(0x80B2140C),
                Color.FromUInt32(0x808E0805),
                Color.FromUInt32(0x806A0103)
            },
            {
                //Pink
                Color.FromUInt32(0x80FDECEF),
                Color.FromUInt32(0x80FBCFD8),
                Color.FromUInt32(0x80F6A0B5),
                Color.FromUInt32(0x80F27396),
                Color.FromUInt32(0x80ED487B),
                Color.FromUInt32(0x80E91E63),
                Color.FromUInt32(0x80C51356),
                Color.FromUInt32(0x80A20B48),
                Color.FromUInt32(0x807E053A),
                Color.FromUInt32(0x805A012B)
            },
            {
                //Purple
                Color.FromUInt32(0x80F7E9F7),
                Color.FromUInt32(0x80EFCAF0),
                Color.FromUInt32(0x80DD9BE0),
                Color.FromUInt32(0x80C96FD1),
                Color.FromUInt32(0x80B449C2),
                Color.FromUInt32(0x809E28B3),
                Color.FromUInt32(0x80871E9E),
                Color.FromUInt32(0x8071168A),
                Color.FromUInt32(0x805C0F75),
                Color.FromUInt32(0x80490A61)
            },
            {
                //Violet
                Color.FromUInt32(0x80F3EDF9),
                Color.FromUInt32(0x80E2D1F4),
                Color.FromUInt32(0x80C4A7E9),
                Color.FromUInt32(0x80A67FDD),
                Color.FromUInt32(0x80885BD2),
                Color.FromUInt32(0x806A3AC7),
                Color.FromUInt32(0x80572FB3),
                Color.FromUInt32(0x8046259E),
                Color.FromUInt32(0x80361C8A),
                Color.FromUInt32(0x80281475)
            },
            {
                //Indigo
                Color.FromUInt32(0x80ECEFF8),
                Color.FromUInt32(0x80D1D8F0),
                Color.FromUInt32(0x80A7B3E1),
                Color.FromUInt32(0x808090D3),
                Color.FromUInt32(0x805E6FC4),
                Color.FromUInt32(0x803F51B5),
                Color.FromUInt32(0x803342A1),
                Color.FromUInt32(0x8028348C),
                Color.FromUInt32(0x801F2878),
                Color.FromUInt32(0x80171D63)
            },
            {
                //Blue
                Color.FromUInt32(0x80EAF5FF),
                Color.FromUInt32(0x80CBE7FE),
                Color.FromUInt32(0x8098CDFD),
                Color.FromUInt32(0x8065B2FC),
                Color.FromUInt32(0x803295FB),
                Color.FromUInt32(0x800064FA),
                Color.FromUInt32(0x800062D6),
                Color.FromUInt32(0x80004FB3),
                Color.FromUInt32(0x80003D8F),
                Color.FromUInt32(0x80002C6B)
            },
            {
                //LightBlue
                Color.FromUInt32(0x80E9F7FD),
                Color.FromUInt32(0x80C9ECFC),
                Color.FromUInt32(0x8095D8F8),
                Color.FromUInt32(0x8062C3F5),
                Color.FromUInt32(0x8030ACF1),
                Color.FromUInt32(0x800095EE),
                Color.FromUInt32(0x80007BCA),
                Color.FromUInt32(0x800063A7),
                Color.FromUInt32(0x80004B83),
                Color.FromUInt32(0x8000355F)
            },
            {
                //Cyan
                Color.FromUInt32(0x80E5F7F8),
                Color.FromUInt32(0x80C2EFF0),
                Color.FromUInt32(0x808ADDE2),
                Color.FromUInt32(0x8058CBD3),
                Color.FromUInt32(0x802CB8C5),
                Color.FromUInt32(0x8005A4B6),
                Color.FromUInt32(0x80038698),
                Color.FromUInt32(0x80016979),
                Color.FromUInt32(0x80004D5B),
                Color.FromUInt32(0x8000323D)
            },
            {
                //Teal
                Color.FromUInt32(0x80E4F7F4),
                Color.FromUInt32(0x80C0F0E8),
                Color.FromUInt32(0x8087E0D3),
                Color.FromUInt32(0x8054D1C1),
                Color.FromUInt32(0x8027C2B0),
                Color.FromUInt32(0x8000B3A1),
                Color.FromUInt32(0x80009589),
                Color.FromUInt32(0x8000776F),
                Color.FromUInt32(0x80005955),
                Color.FromUInt32(0x80003C3A)
            },
            {
                //Green
                Color.FromUInt32(0x80ECF7EC),
                Color.FromUInt32(0x80D0F0D1),
                Color.FromUInt32(0x80A4E0A7),
                Color.FromUInt32(0x807DD182),
                Color.FromUInt32(0x805AC262),
                Color.FromUInt32(0x803BB346),
                Color.FromUInt32(0x8030953B),
                Color.FromUInt32(0x8025772F),
                Color.FromUInt32(0x801B5924),
                Color.FromUInt32(0x80113C18)
            },
            {
                //LightGreen
                Color.FromUInt32(0x80F3F8EC),
                Color.FromUInt32(0x80E3F0D0),
                Color.FromUInt32(0x80C8E2A5),
                Color.FromUInt32(0x80ADD37E),
                Color.FromUInt32(0x8093C55B),
                Color.FromUInt32(0x807BB63C),
                Color.FromUInt32(0x80649830),
                Color.FromUInt32(0x804E7926),
                Color.FromUInt32(0x80395B1B),
                Color.FromUInt32(0x80253D12)
            },
            {
                //Lime
                Color.FromUInt32(0x80F2FAE6),
                Color.FromUInt32(0x80E3F6C5),
                Color.FromUInt32(0x80CBED8E),
                Color.FromUInt32(0x80B7E35B),
                Color.FromUInt32(0x80A7DA2C),
                Color.FromUInt32(0x809BD100),
                Color.FromUInt32(0x807EAE00),
                Color.FromUInt32(0x80638B00),
                Color.FromUInt32(0x80486800),
                Color.FromUInt32(0x802F4600)
            },
            {
                //Yellow
                Color.FromUInt32(0x80FFFDEA),
                Color.FromUInt32(0x80FEFBCB),
                Color.FromUInt32(0x80FDF398),
                Color.FromUInt32(0x80FCE865),
                Color.FromUInt32(0x80FBDA32),
                Color.FromUInt32(0x80FAC800),
                Color.FromUInt32(0x80D0AA00),
                Color.FromUInt32(0x80A78B00),
                Color.FromUInt32(0x807D6A00),
                Color.FromUInt32(0x80534800)
            },
            {
                //Amber
                Color.FromUInt32(0x80FEFBEB),
                Color.FromUInt32(0x80FCF5CE),
                Color.FromUInt32(0x80F9E89E),
                Color.FromUInt32(0x80F6D86F),
                Color.FromUInt32(0x80F3C641),
                Color.FromUInt32(0x80F0B114),
                Color.FromUInt32(0x80C88A0F),
                Color.FromUInt32(0x80A0660A),
                Color.FromUInt32(0x80784606),
                Color.FromUInt32(0x80502B03)
            },
            {
                //Orange
                Color.FromUInt32(0x80FFF8EA),
                Color.FromUInt32(0x80FEEECC),
                Color.FromUInt32(0x80FED998),
                Color.FromUInt32(0x80FDC165),
                Color.FromUInt32(0x80FDA633),
                Color.FromUInt32(0x80FC8800),
                Color.FromUInt32(0x80D26700),
                Color.FromUInt32(0x80A84A00),
                Color.FromUInt32(0x807E3100),
                Color.FromUInt32(0x80541D00)
            },
            {
                //Grey
                Color.FromUInt32(0x80F9F9F9),
                Color.FromUInt32(0x80E6E8EA),
                Color.FromUInt32(0x80C6CACD),
                Color.FromUInt32(0x80A7ABB0),
                Color.FromUInt32(0x80888D92),
                Color.FromUInt32(0x806B7075),
                Color.FromUInt32(0x80555B61),
                Color.FromUInt32(0x8041464C),
                Color.FromUInt32(0x802E3238),
                Color.FromUInt32(0x801C1F23)
            }
        };

        public Color GetColor(int colorIndex, int shadeIndex)
        {
            return Colors[
                Math.Clamp(colorIndex, 0, ColorCount - 1),
                Math.Clamp(shadeIndex, 0, ShadeCount - 1)
            ];
        }

        public int ColorCount => Colors.GetLength(0);

        public int ShadeCount => Colors.GetLength(1);
    }
}