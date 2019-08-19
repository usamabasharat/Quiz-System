using quiz.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace quiz.Controllers
{
    public class HomeController : Controller
    {
        Database1Entities db = new Database1Entities();


        [HttpGet]

        public ActionResult register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult register(TBL_STUDENT svw)
        {
            TBL_STUDENT s = new TBL_STUDENT();
            try
            {
                s.S_NAME = svw.S_NAME;
                s.S_PASSWORD = svw.S_PASSWORD;
                db.TBL_STUDENT.Add(s);
                db.SaveChanges();
                return RedirectToAction("slogin");
            }
            catch (Exception)
            {
                ViewBag.msg = "Data could not be inserted!";
            }
            
            return View();

        }
        public ActionResult Remove1(int id)
        {
            var q = db.TBL_QUESTIONS.ToList();
            foreach(var i in q)
            {
                if (i.QUESTION_ID == id)
                {
                    db.TBL_QUESTIONS.Remove(i);
                    db.SaveChanges();
                    break;
                }
            }
            return RedirectToAction("viewAllQuestions","Home");
        }

        

        public ActionResult Logout()
        {
            Session.Abandon();
            Session.RemoveAll();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult tlogin()
        {

            return View();
        }

        [HttpPost]
        public ActionResult tlogin(TBL_ADMIN a)
        {
            TBL_ADMIN ad = db.TBL_ADMIN.Where(x => x.AD_NAME == a.AD_NAME && x.AD_PASSWORD == a.AD_PASSWORD).SingleOrDefault();
            if (ad != null)
            {
                Session["AD_ID"] = ad.AD_ID;
                return RedirectToAction("Dashboard");
            }
            else
            {
                ViewBag.msg = "Invalid name or password.";
            }
            return View();
        }


        [HttpGet]
        public ActionResult slogin()
        {
            return View();
        }


        [HttpPost]
        public ActionResult slogin(TBL_STUDENT s)
        {
            TBL_STUDENT std = db.TBL_STUDENT.Where(x => x.S_NAME == s.S_NAME && x.S_PASSWORD == s.S_PASSWORD).SingleOrDefault();
            if (std == null)
            {
                ViewBag.msg = "Invalid email or password.";
            }
            else
            {
                Session["STD_ID"] = std.S_ID;
                return RedirectToAction("StudentExam");
            }
            return View();
        }

        
        public ActionResult StudentExam()
        {
            if (Session["STD_ID"] == null)
            {
                return RedirectToAction("slogin");
            }
            return View();
        }

        [HttpPost]
        public ActionResult StudentExam(string room)
        {
          List <TBL_CATEGORY> list = db.TBL_CATEGORY.ToList();
            TempData["score"] = 0;
            foreach(var item in list)
            {
                if (item.CAT_ENCRYPTEDSTRING == room)
                {
                    List<TBL_QUESTIONS> li = db.TBL_QUESTIONS.Where(x => x.Q_FK_CATID == item.CAT_ID).ToList();
                    Queue<TBL_QUESTIONS> queue = new Queue<TBL_QUESTIONS>();
                    foreach (TBL_QUESTIONS a in li)
                    {
                        queue.Enqueue(a);
                    }
                    TempData["examid"] = item.CAT_ID;
                    TempData["questions"] = queue;
                    TempData["score"] = 0;
                    TempData["total"] = 0;
                    //TempData["examid"] = item.CAT_ID;
                    TempData.Keep();
                    return RedirectToAction("QuizStart");
                }
                else
                {
                    ViewBag.error = "No room found...";
                }
            }
            return View();
        }

        public ActionResult QuizStart()
        {
            if (Session["STD_ID"] == null)
            {
                return RedirectToAction("slogin");
            }

            TBL_QUESTIONS q = null;
            if (TempData["questions"] != null)
            {
                Queue<TBL_QUESTIONS> qlist = (Queue<TBL_QUESTIONS>)TempData["questions"];
                if (qlist.Count > 0)
                {
                    q = qlist.Peek();
                    qlist.Dequeue();
                    TempData["questions"] = qlist;
                    TempData.Keep();
                }
                else
                {
                    return RedirectToAction("EndExam");
                }
                
            }
            else
            {
                return RedirectToAction("StudentExam");
            }
            return View(q);
            //if (Session["STD_ID"] == null)
            //{
            //    return RedirectToAction("slogin");
            //}
            //try
            //{
            //    TBL_QUESTIONS q = null;
            //    int examid = Convert.ToInt32(TempData["examid"].ToString());

            //    if (TempData["qid"] == null)
            //    {
            //        q = db.TBL_QUESTIONS.First(x => x.Q_FK_CATID == examid);
            //        TempData["qid"] = ++q.QUESTION_ID;
            //    }
            //    else
            //    {
            //        int qid = Convert.ToInt32(TempData["qid"].ToString());
            //        q = db.TBL_QUESTIONS.Where(x => x.QUESTION_ID == qid && x.Q_FK_CATID == examid).SingleOrDefault();
            //        TempData["qid"] = ++q.QUESTION_ID;
            //    }
            //    TempData.Keep();
            //    return View(q);
            //}
            //catch (Exception)
            //{
            //    return RedirectToAction("EndExam");
            //}
            
        }

        [HttpPost]

        public ActionResult QuizStart(TBL_QUESTIONS q)
        {
            try
            {
                string correctans = null;

                if (q.OPA != null)
                {
                    correctans = "A";
                }
                else if (q.OPB != null)
                {
                    correctans = "B";
                }
                else if (q.OPC != null)
                {
                    correctans = "C";
                }
                else if (q.OPD != null)
                {
                    correctans = "D";
                }

                if (correctans.Equals(q.COP))
                {
                    TempData["score"] = Convert.ToInt32(TempData["score"]) + 5;
                }

                TempData["total"] = Convert.ToInt32(TempData["total"]) + 5;
                TempData.Keep();
                return RedirectToAction("QuizStart");
            }
            catch (Exception)
            {

                return RedirectToAction("QuizStart");
            }
            
        }

        public ActionResult EndExam()
        {

            return View();
        }

        public ActionResult viewAllQuestions(int?id)
        {
            if (Session["AD_ID"] == null)
            {
                return RedirectToAction("tlogin");
            }
            if (id == null)
            {
                return RedirectToAction("Dashboard");

            }
            return View(db.TBL_QUESTIONS.Where(x =>x.Q_FK_CATID == id).ToList());
        }

        public ActionResult Dashboard()
        {
            if (Session["AD_ID"] == null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpGet]
        public ActionResult Addcategory()
        {
            if (Session["AD_ID"] == null)
            {
                return RedirectToAction("Index");
            }
           // Session["AD_ID"] = 1;
            int adid = Convert.ToInt32(Session["AD_ID"].ToString());
            List<TBL_CATEGORY> li = db.TBL_CATEGORY.Where(x => x.CAT_FK_ADID == adid).OrderByDescending(x => x.CAT_ID).ToList();
            ViewData["list"] = li;

            return View();
        }

        [HttpPost]
        public ActionResult Addcategory(TBL_CATEGORY cat)
        {
            try
            {
                Random r = new Random();
                List<TBL_CATEGORY> li = db.TBL_CATEGORY.OrderByDescending(x => x.CAT_ID).ToList();
                ViewData["list"] = li;
                TBL_CATEGORY c = new TBL_CATEGORY();
                c.CAT_NAME = cat.CAT_NAME;
                c.CAT_ENCRYPTEDSTRING = crypto.Encrypt(cat.CAT_NAME.Trim() + r.Next().ToString(), true);
                c.CAT_FK_ADID = Convert.ToInt32(Session["AD_ID"].ToString());
                db.TBL_CATEGORY.Add(c);
                db.SaveChanges();


                return RedirectToAction("Addcategory");
            }
            catch (Exception)
            {
                return RedirectToAction("Addcategory");

            }
            
        }

        [HttpGet]
        public ActionResult Addquestion()
        {
            int sid = Convert.ToInt32(Session["AD_ID"]);
            List<TBL_CATEGORY> li = db.TBL_CATEGORY.Where(x => x.CAT_FK_ADID == sid).ToList();
            ViewBag.list = new SelectList(li, "CAT_ID", "CAT_NAME");
            return View();
        }

        [HttpPost]
        public ActionResult Addquestion(TBL_QUESTIONS q)
        {
            try
            {
                int sid = Convert.ToInt32(Session["AD_ID"]);
                List<TBL_CATEGORY> li = db.TBL_CATEGORY.Where(x => x.CAT_FK_ADID == sid).ToList();
                ViewBag.list = new SelectList(li, "CAT_ID", "CAT_NAME");

                TBL_QUESTIONS QA = new TBL_QUESTIONS();
                QA.Q_TEXT = q.Q_TEXT;
                QA.OPA = q.OPA;
                QA.OPB = q.OPB;
                QA.OPC = q.OPC;
                QA.OPD = q.OPD;
                QA.COP = q.COP;
                QA.Q_FK_CATID = q.Q_FK_CATID;


                db.TBL_QUESTIONS.Add(QA);
                db.SaveChanges();
                TempData["msg"] = "Question has been added successfully!";
                TempData.Keep();
                return RedirectToAction("Addquestion");

            }
            catch (Exception)
            {
                return RedirectToAction("Addquestion");


            }


        }

        public ActionResult Index()

        {
            if (Session["AD_ID"] != null)
            {
                return RedirectToAction("Dashboard");
            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}