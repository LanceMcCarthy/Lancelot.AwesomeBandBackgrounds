# Awesome Band Backgrounds

The ultimate Microsoft Band companion app. Access millions of amazing, high definition wallpaper photos and dozens of professional palette-based themes.

![AwesomeBB_Combined_SS](https://user-images.githubusercontent.com/3520532/193422604-5aafed7c-1e41-407f-a3e2-95f5e122c25a.jpg)

## Installation Options

- [Microsoft Store](https://www.microsoft.com/en-us/p/awesome-band-backgrounds/9nblggh3g0sn)
- Source Code: See the [Developer Note](#developer-note) section for important information.

## Features

- **Multi-device support**: Automatic detection for Microsoft Band 1 or Microsoft Band 2
- **Stylish Themes**:
  - Pick your theme colors from professional palettes
  - Theme History to quickly set any past themes you want to use again
- **Image sources**: Millions of photos at your fingertips in an infinite scrolling list from a variety of available sources:
  - Flicker
  - 500px
  - Bing
  - OneDrive
  - Local files on your phone
- **Preview**: Get an instant preview of how image looks on your band before you apply it
- **Effects**: Crop and apply dozens of amazing effects to photos
- **Speed**: search, pick and set your background in seconds.
- **Favorites**: Full Favorites list experience, with the ability to set background even faster
- **Background Updates**: The background task can update the image automatically for you
- **Backups**: Cloud backup and restore favorites from cloud

## Developer Note

I have open sourced this to give back to the community the hundreds and hundreds of hours of work over the years. From custom controls, to UI features, these are available in the source in the `src/` subfolder. You can compile the projects locally and interact with everything in the VS 2022 designer. While preparing this for open source, I have put a lot of recent work into compatibility with modern tools.

However, if you want to actually run and use this app from code (instead of installing from the Store), you will need to get your own API keys with the various services. These values are set in the various constants files in the [src/BandCentral.Models/Secrets](https://github.com/LanceMcCarthy/Lancelot.AwesomeBandBackgrounds/tree/main/src/BandCentral.Models/Secrets) folder.
