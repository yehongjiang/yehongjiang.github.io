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
        //登录界面
        public ActionResult Index()
        {
            return View();
        }
        //后台框架界面
        public ActionResult Iframe()
        {
            return View();
        }
        //欢迎页
        public ActionResult Welcome()
        {
            return View();
        }
        public void Login()
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string username = Request.Form["username"].Replace("'", "");
            string password = Request.Form["password"].Replace("'","");
            string sql = "select id from dm_user where name = '" + username  + "' and password = '" + getMd5Hash(password) + "';";
            SqlCommand cmd = new SqlCommand(sql, con);
            string result = Convert.ToString(cmd.ExecuteScalar());   
            con.Close();

            if (result != "")
            {
                Session["user_id"] = result;
                Session["username"] = Request.Form["username"];
                con.Open();
                sql = "select real_name from dm_user where name = '" + Request.Form["username"] + "' and password = '" + getMd5Hash(password) + "';";
                cmd = new SqlCommand(sql, con);
                Session["real_name"] = cmd.ExecuteScalar().ToString();
                con.Close();             
                Response.Redirect("/Home/Iframe",false);
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