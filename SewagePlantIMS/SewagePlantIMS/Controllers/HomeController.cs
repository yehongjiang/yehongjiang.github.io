using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace SewagePlantIMS.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public void Login()
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string password = Request.Form["password"];
            string sql = "select count(*) from dm_user where name = '" + Request.Form["username"] + "' and password = '" + getMd5Hash(password) + "';";
            SqlCommand cmd = new SqlCommand(sql, con);
            int result = (int)cmd.ExecuteScalar();
            con.Close();

            if (result > 0)
            {
                Session["username"] = Request.Form["username"];
                Response.Redirect("/ElectricManage/Index");
                con.Open();
                sql = "select real_name from dm_user where name = '" + Request.Form["username"] + "' and password = '" + getMd5Hash(password) + "';";
                cmd = new SqlCommand(sql, con);
                Session["real_name"] = cmd.ExecuteScalar().ToString();
                con.Close();
            }
            else
            {
                Response.Write("<script>alert('用户名或密码错误！！');window.location.href('Index');</script>");
            }
            
        }
        /// MD5加密
        public  string getMd5Hash(string input)
        {
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            string result = sBuilder.ToString().Substring(8, 16);
            return result;

        }

    }
}