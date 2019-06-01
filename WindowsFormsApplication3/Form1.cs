using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms.ToolTips;
using System.Web;
using System.Device.Location;
//using System.Web.UI.WebControls;
using System.Net;
using System.Xml;
using System.Collections.Specialized;
using System.Diagnostics;
namespace WindowsFormsApplication3
{
    
    public partial class Form1 : Form
    {
            

        public Form1()
        {

            InitializeComponent();
        }
        
        private void button1_Click(object sender, EventArgs e)//search donors
        {
            listed.c=0;
            Process.Start("location finder");
            StreamReader str = new StreamReader("latlon.txt");
            string abb=str.ReadLine();
            string acc=str.ReadLine();
            PointLatLng et = new PointLatLng(Convert.ToDouble(abb),Convert.ToDouble( acc));
            MyMap.Overlays.Clear();
            listed.a = 0;
            listed.b = 0;
            string a = seach_box.Text;
            if(radioButton1.Checked)
            {
                list x=new list();
                Tnode l;
                MessageBox.Show(a);
                x.insert(a);
                l = x.gethead();
                int len = x.length;
                if (len == 0)
                    MessageBox.Show("No donors found!");
                else
                {
                    gMapMarkerMaker(l, et, len);
                }
            }
            else if(radioButton2.Checked)
            {
                BST y = new BST();
                list x=new list();
                x.insert(a);
                Tnode l;
                l = x.gethead();
                for (int i = 0; i < listed.a; i++)
                {
                    y.insert(l.data, et);
                    l = l.next;
                }
                Donor donor = y.displaynearest();
                if (donor != null)
                {
                    listed.a = 0;
                    gMapMarkerMaker(donor, et);
                }
            }
            
        }
        void gMapMarkerMaker(node donor,PointLatLng et)
        {
            MyMap.Position = et;
            MyMap.Overlays.Clear();
            MyMap.Zoom = 12;
            recursiveBST(donor);
        }
        void recursiveBST(node root)
        {
            if(root==null)
            {
                return;
            }
            recursiveBST(root.left);
            GMapOverlay markersOverlay = new GMapOverlay("markers");
            GMarkerGoogle marker;
            TimeSpan st = new TimeSpan(58, 0, 0, 0);
            if (root.data.TimePassed() > st)
                marker = new GMarkerGoogle(root.data.getlatlog(), GMarkerGoogleType.green_small);//if the donor have passed 58 days after giving blood then the marker will be green else red
            else
                marker = new GMarkerGoogle(root.data.getlatlog(), GMarkerGoogleType.red_small);
            markersOverlay.Markers.Add(marker);
            marker.ToolTip = new GMapRoundedToolTip(marker);
            marker.ToolTipText = root.data.getName().ToString() + "\n" + root.data.getBloodGroup().ToString() + "\n" + root.data.getEmail().ToString();
            MyMap.Overlays.Add(markersOverlay);
            recursiveBST(root.right);
        }
        void gMapMarkerMaker(Tnode donor, PointLatLng et,int length)//show markers
        {
            MyMap.Position = et;
            try
            {
                MyMap.Overlays.Clear();
                MyMap.Zoom = 14;
                TimeSpan st = new TimeSpan(58, 0, 0, 0);
                GMapOverlay markersOverlay = new GMapOverlay("markers");
                GMarkerGoogle[] marker=new GMarkerGoogle[length];
                Tnode ptr;
                ptr=donor;
                int i = 0;
                while(ptr!=null)
                {
                    if (ptr.data.blood_group.ToString() == seach_box.Text.ToString())
                    {
                        if (ptr.data.TimePassed() > st)
                            marker[i] = new GMarkerGoogle(ptr.data.getlatlog(), GMarkerGoogleType.green_small);//if the donor have passed 58 days after giving bloob then the marker will be green else red
                        else
                            marker[i] = new GMarkerGoogle(ptr.data.getlatlog(), GMarkerGoogleType.red_small);
                        markersOverlay.Markers.Add(marker[i]);
                        marker[i].ToolTip = new GMapRoundedToolTip(marker[i]);
                        marker[i].ToolTipText = ptr.data.getName().ToString() + "\n" + ptr.data.getBloodGroup().ToString() + "\n" + ptr.data.getEmail().ToString();
                        MyMap.Overlays.Add(markersOverlay);
                        i++;
                        listed.b++;
                        
                    }
                    ptr = ptr.next;
                }
                if (i == 0)
                    MessageBox.Show("No donors found!");
                else
                    MessageBox.Show(i.ToString() + " donors found!");

            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show(ex.ToString(), "An exception has occured!"); 
            }
            
        }
        void gMapMarkerMaker (Donor donor,PointLatLng et)
        {
            MyMap.Overlays.Clear();
            MyMap.Zoom = 14;
            TimeSpan st = new TimeSpan(58, 0, 0, 0);
            GMapOverlay markersOverlay = new GMapOverlay("markers");
            GMarkerGoogle marker;
            if (donor.TimePassed() > st)
                marker = new GMarkerGoogle(donor.getlatlog(), GMarkerGoogleType.green_small);//if the donor have passed 58 days after giving bloob then the marker will be green else red
            else
                marker = new GMarkerGoogle(donor.getlatlog(), GMarkerGoogleType.red_small);
            markersOverlay.Markers.Add(marker);
            marker.ToolTip = new GMapRoundedToolTip(marker);
            marker.ToolTipText = donor.getName().ToString() + "\n" + donor.getBloodGroup().ToString() + "\n" + donor.getEmail().ToString();
            MyMap.Overlays.Add(markersOverlay);
            listed.b++;
        }
        void gMApMarkerMaker(Donor[] all,string query)//show markers
        {
            listed.b = 0;
            double lat = 33.6425330;
            double lng = 72.9901891;
            MyMap.Position = new PointLatLng(lat, lng);
            try
            {
                MyMap.Overlays.Clear();
                string d = query.ToUpper();
                TimeSpan st = new TimeSpan(58, 0, 0, 0);
                int count = 0;
                GMapOverlay markersOverlay = new GMapOverlay("markers");
                GMarkerGoogle[] marker = new GMarkerGoogle[all.Length];
                for (int i = 0; i < all.Length;i++ )
                {
                    if (all[i].getBloodGroup() == d)
                    {

                        if (all[i].TimePassed() > st)
                            marker[i] = new GMarkerGoogle(all[i].getlatlog(), GMarkerGoogleType.green_small);//if the donor have passed 58 days after giving bloob then the marker will be green else red
                        else
                            marker[i] = new GMarkerGoogle(all[i].getlatlog(), GMarkerGoogleType.red_small);
                        markersOverlay.Markers.Add(marker[i]);
                        marker[i].ToolTip = new GMapRoundedToolTip(marker[i]);
                        marker[i].ToolTipText = all[i].getName().ToString() + "\n" + all[i].getBloodGroup().ToString() + "\n" + all[i].getEmail().ToString();
                        MyMap.Overlays.Add(markersOverlay);
                        count++;
                        listed.b++;
                    }
                    if (d == "SHOW ALL")
                    {
                        listed.b++;
                        if (all[i].TimePassed() > st)
                        {
                            marker[i] = new GMarkerGoogle(all[i].getlatlog(), GMarkerGoogleType.green_small);//if the donor have passed 58 days after giving bloob then the marker will be green else red
                        }
                        else
                            marker[i] = new GMarkerGoogle(all[i].getlatlog(), GMarkerGoogleType.red_small);
                        markersOverlay.Markers.Add(marker[i]);
                        marker[i].ToolTip = new GMapRoundedToolTip(marker[i]);
                        marker[i].ToolTipText = all[i].getName().ToString() + "\n" + all[i].getBloodGroup().ToString() + "\n" + all[i].getEmail().ToString();
                        MyMap.Overlays.Add(markersOverlay);
                        count++; 
                    }
                }
                if (count == 0)
                    MessageBox.Show("No donors found!");
                else
                    MessageBox.Show(count.ToString() + " donors found!");

            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show(ex.ToString(), "An exception has occured!");
            }
        }

        private void MyMap_Load(object sender, EventArgs e)//load map
        {
            MyMap.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;//setting map provider
            double lat = 33.6425330;
            double lng = 72.9901891;
            MyMap.Position = new PointLatLng(lat,lng);//setting map starting position
            MyMap.DragButton = MouseButtons.Left;
            MyMap.ShowCenter = false;
            ZoomBar.Value = Convert.ToInt32(MyMap.Zoom) - 2;
        }

        private void seach_box_TextChanged(object sender, EventArgs e)
        {
          
        }
        public void Gmarker_Click_1(object sender,EventArgs e)
        {
            if(listed.c!=0)
            {
                return;
            }
            
            GMarkerGoogle Mark = sender as GMarkerGoogle;
            listed.mail= Mark.ToolTipText;
            Form2 fom = new Form2();
            fom.Show();
            listed.c++;

            

        }
        private void button1_Click_1(object sender, EventArgs e)//addint data to the database file
        {       string a;
                TextWriter str = new StreamWriter("DATA.txt", true);
            try
            {
                

                if (Set_Blood_Group.Text.Length != 0 && set_Day.Text.Length != 0 && set_Email.Text.Length != 0 && Set_lat.Text.Length != 0
                    && Set_lng.Text.Length != 0 && Set_month.Text.Length != 0 && set_Name.Text.Length != 0 && set_Phone_Number.Text.Length != 0
                    && Set_year.Text.Length != 0 
                    && Convert.ToInt32(set_Day.Text) > 0 && Convert.ToInt32(set_Day.Text) <= 31 
                    && Convert.ToInt32(Set_month.Text) > 0&& Convert.ToInt32(Set_month.Text) <= 12 
                    && Convert.ToInt32(Set_year.Text) > 0 && Convert.ToInt32(Set_year.Text) <= DateTime.Now.Year   
                    && Convert.ToDouble(Set_lat.Text) > 0 && Convert.ToDouble(Set_lat.Text) < 180
                    && Convert.ToDouble(Set_lng.Text) > 0 && Convert.ToDouble(Set_lng.Text) < 180
                   )
                {
                    a = set_Name.Text.ToString();
                    str.WriteLine(a.ToString());
                    a = set_Email.Text.ToString();
                    str.WriteLine(a.ToString());
                    a = set_Phone_Number.Text.ToString();
                    str.WriteLine(a.ToString());
                    a = Set_Blood_Group.Text.ToString();
                    str.WriteLine(a.ToUpper().ToString());
                    a = set_Day.Text.ToString();
                    str.WriteLine(a.ToString());
                    a = Set_month.Text.ToString();
                    str.WriteLine(a.ToString());
                    a = Set_year.Text.ToString();
                    str.WriteLine(a.ToString());
                    a = Set_lat.Text.ToString();
                    str.WriteLine(a.ToString());
                    a = Set_lng.Text.ToString();
                    str.WriteLine(a.ToString());

                    MessageBox.Show("Your information has been added to the system. Thank you!");
                }
                else
                {
                    MessageBox.Show("Some fields are empty or have incorrect data. Please try again!");

                }
                
            }

            catch (IOException ex)
            {
                MessageBox.Show(ex.ToString(), "error");
            }
            catch(FormatException ex)
            {
                MessageBox.Show("Some fields are empty or have incorrect data. Please try again!"+ex.ToString());
            }
            str.Close();
            set_Day.Clear();//clearing all fields
            Set_Blood_Group.Clear();
            set_Email.Clear();
            Set_lat.Clear();
            Set_lng.Clear();
            Set_month.Clear();
            set_Name.Clear();
            set_Phone_Number.Clear();
            Set_year.Clear();

        }

        private void ZoomBar_Scroll(object sender, EventArgs e)
        {
            MyMap.Zoom = ZoomBar.Value + 2;
        }

        private void tabPage2_Click(object sender, EventArgs e)
        {
            Process.Start("location finder");
            StreamReader str = new StreamReader("latlon.txt");
            string abb = str.ReadLine();
            string acc = str.ReadLine();
            PointLatLng et = new PointLatLng(Convert.ToDouble(abb), Convert.ToDouble(acc));
            Set_lat.Text = et.Lat.ToString();
            Set_lng.Text = et.Lng.ToString();
        }

        private void tabPage3_Click(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}