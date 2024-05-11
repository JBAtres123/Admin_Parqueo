
namespace Admin_Parqueo.Clases
{
    public class Vehiculo
    {
        public int VehiculoID { get; set; }
        public int UsuarioID { get; set; }
        public string Placa { get; set; }
        public string TipoVehiculo { get; set; }
        public string Modelo { get; set; }
        public string Color { get; set; }
        public DateTime FechaIngreso { get; internal set; }
    }

}
