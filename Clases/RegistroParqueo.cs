namespace Admin_Parqueo.Clases
{
    public class RegistroParqueo
    {
        public int RegistroID { get; set; }
        public int VehiculoID { get; set; }
        public int EspacioID { get; set; }
        public DateTime HoraEntrada { get; set; }
        public DateTime? HoraSalida { get; set; }
        public bool Emergencia { get; set; }
        public string TipoGenero { get; set; }
        public string Estado {  get; set; }

    }

}
