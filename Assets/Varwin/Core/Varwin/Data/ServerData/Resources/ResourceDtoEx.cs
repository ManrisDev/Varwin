using System.Linq;

namespace Varwin.Data.ServerData
{
    public static class ResourceDtoEx
    {
        /// <summary>
        /// Id 1 = png, Id 2 = jpeg
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsPicture(this ResourceDto self) => ResourceFormatCodes.ImageFormats.Contains(self.Format);

        /// <summary>
        /// Id 3 = txt 
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsText(this ResourceDto self) => ResourceFormatCodes.TextFormats.Contains(self.Format);

        public static bool IsModel(this ResourceDto self) => ResourceFormatCodes.ModelFormats.Contains(self.Format);
        
        public static bool IsAudio(this ResourceDto self) => ResourceFormatCodes.AudioFormats.Contains(self.Format);
        
        public static bool IsVideo(this ResourceDto self) => ResourceFormatCodes.VideoFormats.Contains(self.Format);
    }
}