using Hydra;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using OdblokowanieZS;
using System.Collections.Generic;
using static OdblokowanieZS.ZS;
using System.Text;
using System;
using System.Data.SqlClient;
using System.Data;
using cdn_api;

[assembly: CallbackAssemblyDescription(Name: "ONE-013 Odblokowanie zamówienia",
                                           Description: "Odblokowanie podświetlonego dokumentu ZS",
                                           Author: "OneSupport",
                                           Version: "1.0",
                                           SystemVersion: "2021.0.2",
                                           Created: "17-10-2022")]


namespace OdblokowanieZS
{
    [SubscribeProcedure(Procedures.ZaNLista, "Odblokowanie ZS")]
    public class ZSnaFS_Class : Callback
    {
        private int XLWersjaAPI = 20202;
        private int XLTrybWsadowy = 1;
        private int XLWinieta = -1;
        private int XLUtworzWlasnaSesje = 1;
        private int sesja = -1;
        private string XLSQLServer = "192.168.1.202";
        private string XLSQLDatabase = "Tuflowers_20220908";//"ERPXL_PoloMarket"
        private string XLUser = "ADMIN"; //
        private string XLPass = "NE!w0rk12!";//"Dupa2003!";
        private ClaWindow RapButton;
        private ClaWindow zakladkaTabWewnetrzne;
        private ClaWindow filtrBtn;
        private ClaWindow listaPoz;
        ZS zS;
        int listID;
        public override void Init()
        {

            AddSubscription(true, 0, Events.JustAfterWindowOpening, new TakeEventDelegate(OnOpenWindow));
            AddSubscription(false, 0, Events.ResizeWindow, new TakeEventDelegate(Resize));
        }



        private bool OnOpenWindow(Procedures ProcId, int ControlId, Events Event)
        {

            ClaWindow Parent = GetWindow();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Rectangle WndBnd = GetWindow().Bounds;
            zakladkaTabWewnetrzne = GetWindow().AllChildren["?TabOdb"];
            listaPoz = GetWindow().AllChildren["?ListaZam"];
            RapButton = zakladkaTabWewnetrzne.Children.Add(ControlTypes.button);
            RapButton.Visible = true;
            //RapButton.TextRaw = "";
            RapButton.ToolTipRaw = "Odblokowanie podświetlonego dokumentu ZS";
            RapButton.IconRaw = "C:\\OneSupport\\klodka.ico";

            filtrBtn = GetWindow().AllChildren["?BRW1::TotalWorking"];                            //pobiera współrzędne przycisku zapisz
            RapButton.Bounds = new Rectangle(filtrBtn.Bounds.X - 46, filtrBtn.Bounds.Y, 20, 15);
            listID = GetWindow().AllChildren["?ListaZam"].Id;      //tu sprawdzic


            RapButton.OnBeforeAccepted += new TakeEventDelegate(zaczytaj_OnAfterAccepted);
            RapButton.OnAfterMouseDown += new TakeEventDelegate(BtnOnMouseDown);
            //pobrane z pola w zakładce Raporty
            return true;
        }

        public bool Resize(Procedures ProcId, int ControlId, Events Event)
        {
            filtrBtn = GetWindow().AllChildren["?BRW1::TotalWorking"];                            //pobiera współrzędne przycisku zapisz
            RapButton.Bounds = new Rectangle(filtrBtn.Bounds.X - 46, filtrBtn.Bounds.Y, 20, 15);

            return true;
        }

        bool zaczytaj_OnAfterAccepted(Procedures ProcedureId, int ControlId, Events Event)
        {
           
            Runtime.WindowController.PostEvent(listaPoz.Id, Events.NewSelection);

            zS = new ZS(Convert.ToInt32(ZamNag.ZaN_GIDNumer), Convert.ToInt32(ZamNag.ZaN_GIDFirma), ZamNag.ZaN_GIDLp, ZamNag.ZaN_GIDTyp);


            return true;
        }

        public bool BtnOnMouseDown(Procedures ProcId, int ControlId, Events Event)
        {

            try
            {


                
                string connectionString = "";
                if (Runtime.ConfigurationDictionary.Baza.ToString().Substring(0, 24) == "ERPXL_Tuflowers_20220908")
                    connectionString = @"Data Source = TF-SQL; Initial Catalog = ERPXL_Tuflowers_20220908; User ID = onesupport; Password=Ancr3d0$$db";
                else if (Runtime.ConfigurationDictionary.Baza.ToString().Substring(0, 22) == "ERPXL_Tuflowers_Mobile")
                    connectionString = @"Data Source = TF-SQL; Initial Catalog = ERPXL_Tuflowers_Mobile; User ID = onesupport; Password=Ancr3d0$$db";
                else if(Runtime.ConfigurationDictionary.Baza.ToString().Substring(0, 15) == "ERPXL_Tuflowers")
                    connectionString = @"Data Source = TF-SQL; Initial Catalog = ERPXL_Tuflowers; User ID = onesupport; Password=Ancr3d0$$db";

                string updateAktywny = "UPDATE cdn.ZamNag set zaN_Aktywny=0 where ZaN_GIDFirma=" + zS.GIDFirma + " and ZaN_GIDNumer=" + zS.GIDNumer + " and ZaN_GIDTyp=" + zS.GIDTyp + " and ZaN_GIDLp=" + zS.GIDLp;
                Update(updateAktywny, connectionString);

                string nrDokQuery = "SELECT cdn.NumerDokumentu(ZaN_GIDTyp, 0, ZaN_ZamTyp, ZaN_ZamNumer, ZaN_ZamRok, ZaN_ZamSeria, ZaN_ZamMiesiac) from cdn.ZamNag WHERE  ZaN_GIDFirma=" + zS.GIDFirma + " and ZaN_GIDNumer=" + zS.GIDNumer + " and ZaN_GIDTyp=" + zS.GIDTyp + " and ZaN_GIDLp=" + zS.GIDLp;
               //MessageBox.Show("Update string: "+updateAktywny );
                //MessageBox.Show(Runtime.ConfigurationDictionary.HASPDataAplikacji.ToString());
                string insertOdb = "INSERT INTO [CDN].[ONE_OdblokowaneZS] ([Dok_GidNumer] ,[Dok_GidFirma] ,[Dok_GidTyp] ,[Dok_GidLp], [Dok_Nazwa] , [Data], [Ope_GidNumer]) VALUES (" + zS.GIDNumer + " ," + zS.GIDFirma + " , " + zS.GIDTyp + " , " + zS.GIDLp 
                                +" , '"+ConnectToSQL(connectionString, nrDokQuery) +"' , '"+System.DateTime.Now.ToString("s") +"' , "+ Runtime.ConfigurationDictionary.NumerOperatora.ToString() + ")";
                InsertInto(insertOdb, connectionString);
                MessageBox.Show("Odblokowano dokument ZS", "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Information);


            }
            catch(Exception e)
            {
                MessageBox.Show("Błąd odblokowywanie ZS: "+e.ToString(), "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            
            
            return true;
        }


        DateTime zmianaDaty(int clarionDate, string connection_string)
        {
            string nowaData="select dateadd(s, "+clarionDate+", '19900101')";
            return Convert.ToDateTime(ConnectToSQL(connection_string, nowaData));
        }

        public string ConnectToSQL(string connection_string, string query)
        {
            string str = "";
            try
            {
                using (SqlConnection connection = new SqlConnection(connection_string))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(query, connection);
                    DataSet dataSet = new DataSet();
                    adapter.Fill(dataSet, "Query");
                    if (dataSet.Tables.Count == 0)
                    {
                        return "";
                    }
                    foreach (DataRow row in (InternalDataCollectionBase)dataSet.Tables[0].Rows)
                    {
                        str = row[0].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                str = str + "Bład SQL:" + ex.ToString() + "\n";
            }
            return str;
        }

        public string Update(string sql, string connection_string)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connection_string))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    connection.Open();
                    adapter.UpdateCommand = new SqlCommand(sql, connection);
                    adapter.UpdateCommand.ExecuteNonQuery();
                    //MessageBox.Show("Row updated !! ");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return sql;
        }

        public string InsertInto(string sql, string connection_string)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connection_string))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    connection.Open();
                    adapter.InsertCommand = new SqlCommand(sql, connection);
                    adapter.InsertCommand.ExecuteNonQuery();
                    //MessageBox.Show("Row inserted !! ");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return sql;
        }


        
        public override void Cleanup()
        {

        }

    }
}
