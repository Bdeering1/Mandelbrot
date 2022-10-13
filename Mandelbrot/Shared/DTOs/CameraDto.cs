namespace Mandelbrot.Shared.DTOs
{
    public class CameraDto
    {
        public int zoom { get; set; }
        public double posX { get; set; }
        public double posY { get; set; }

        public CameraDto(int zoom, double posX, double posY)
        {
            this.zoom = zoom;
            this.posX = posX;
            this.posY = posY;
        }

        public CameraDto() { }
    }
}
