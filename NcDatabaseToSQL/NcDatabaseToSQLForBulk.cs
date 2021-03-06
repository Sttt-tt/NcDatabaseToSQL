﻿using ERP8.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Web.Services;

namespace NcDatabaseToSQL
{
    public class NcDatabaseToSQLForBulk
    {
        //开始时间
        string startTime = DateTime.Now.AddDays(1 - DateTime.Now.Day).AddMonths(-1).ToString("yyyy-MM-dd");
        //结束时间
        string endTime = DateTime.Now.AddDays(1 - DateTime.Now.Day).AddDays(-1).ToString("yyyy-MM-dd");
        private static string connectionString = ConfigurationManager.ConnectionStrings["U8ConnBluk"].ToString();
        private static string u8SVApiUrl = ConfigurationManager.ConnectionStrings["U8SVApiUrlBluk"].ToString();

        string msg = "";
        public string NcDatabaseToSQL()
        {
            Stopwatch stopwatch = new Stopwatch();
            Stopwatch stopwatch1 = new Stopwatch();
            // 开始监视代码运行时间
            stopwatch.Start();
            //采购入库
            msg = GetPurchaseinToSql() + "/";
            //GetU8SVApiUrlApi("cgrkapi");
            //材料出库
            msg = msg + GetMaterialToSql() + "/";
            //GetU8SVApiUrlApi("clckapi");
            //产成品入库
            msg = msg + GetFinprodInToSql() + "/";
            //GetU8SVApiUrlApi("ccprkapi");
            //其他入库
            msg = msg + GetIAi4billToSql() + "/";
            //GetU8SVApiUrlApi("qtrkdapi");
            //其他出库
            msg = msg + GetIAi7billToSql() + "/";
            //GetU8SVApiUrlApi("qtckdapi");
            //形态转换
            msg = msg + GetIcTransformHToSql() + "/";
            //GetU8SVApiUrlApi("xtzhdapi");
            //调拨单
            msg = msg + GetIcWhstransHToSql() + "/";
            //GetU8SVApiUrlApi("dbdapi");
            //销售出库
            msg = msg + GetSaleOutToSql() + "/";
            //GetU8SVApiUrlApi("fhdapi");
            //采购发票
            msg = msg + GetPurchaseInvoicesToSql() + "/";
            //GetU8SVApiUrlApi("cgfpapi");
            //销售发票
            msg = msg + GetSoSaleinvoiceToSql() + "/";
            //GetU8SVApiUrlApi("xsfpapi");
            DateTime t2 = DateTime.Now;
            stopwatch.Stop();

            stopwatch1.Start();
            GetU8SVApiUrlApi();
            stopwatch1.Stop(); // 停止监视
            TimeSpan timespan = stopwatch.Elapsed; // 获取当前实例测量得出的总时间
            TimeSpan timespan1 = stopwatch1.Elapsed; // 获取当前实例测量得出的总时间
            double hours = timespan.TotalHours; // 总小时
            double minutes = timespan.TotalMinutes; // 总分钟
            double seconds = timespan.TotalSeconds; // 总秒数
            double seconds1 = timespan1.TotalSeconds; // 总秒数
            double milliseconds = timespan.TotalMilliseconds; // 总毫秒数



            msg = "数据导入执行时间:" + (seconds.ToString()) + "--" + msg + "接口执行时间:" + seconds1.ToString();

            return msg;
        }


        public void GetU8SVApiUrlApi()
        {
            //msg = GetPurchaseinToSql() + "/";
            GetU8SVApiUrlApi("cgrkapi");
            //材料出库
            //msg = msg + GetMaterialToSql() + "/";
            GetU8SVApiUrlApi("clckapi");
            //产成品入库
            //msg = msg + GetFinprodInToSql() + "/";
            GetU8SVApiUrlApi("ccprkapi");
            //其他入库
            //msg = msg + GetIAi4billToSql() + "/";
            GetU8SVApiUrlApi("qtrkdapi");
            //其他出库
            //msg = msg + GetIAi7billToSql() + "/";
            GetU8SVApiUrlApi("qtckdapi");
            //形态转换
            //msg = msg + GetIcTransformHToSql() + "/";
            GetU8SVApiUrlApi("xtzhdapi");
            //调拨单
            //msg = msg + GetIcWhstransHToSql() + "/";
            GetU8SVApiUrlApi("dbdapi");
            //销售出库
            //msg = msg + GetSaleOutToSql() + "/";
            GetU8SVApiUrlApi("fhdapi");
            //采购发票
            //msg = msg + GetPurchaseInvoicesToSql() + "/";
            GetU8SVApiUrlApi("cgfpapi");
            //销售发票
            //msg = msg + GetSoSaleinvoiceToSql() + "/";
            GetU8SVApiUrlApi("xsfpapi");
        }



        /// <summary>
        /// 从nc获取采购入库数据插入到sql
        /// 创建人：lvhe
        /// 创建时间：2019年10月23日 00:20:34
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        private string GetPurchaseinToSql()
        {
            string result = "";
            string createSql = "";
            string tableExist = "";
            int existResult = 0;
            string msg = "";
            string sql = "";
            int updateCount = 0;
            StringBuilder strbu = new StringBuilder();
            string strGetOracleSQLIn = "";
            try
            {
                //判断当前表是否存在 1存在 0 不存在
                tableExist = "if object_id( 'Rdrecord01') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);
                if (existResult == 0)
                {
                    //获取采购入库头数据
                    sql = " select distinct A.cgeneralhid ID, A.vbillcode code, A.dbilldate ddate, A2.code cwhcode, A3.code cvencode,A4.code cvenclass,A3.name cvenname,A1.CODE cdepcode,CASE WHEN A.freplenishflag='N' THEN 0 ELSE 1 END AS isRed,A.vnote remark, A.modifiedtime ts FROM ic_purchasein_h A left join ic_purchasein_b A5 on A5.cgeneralhid = A.cgeneralhid  left join org_dept A1 on A1.pk_dept = A.cdptid left join bd_stordoc A2 on A2.pk_stordoc = A.cwarehouseid left join bd_supplier A3 on A3.pk_supplier = A.cvendorid left join bd_supplierclass A4 on A4.pk_supplierclass=A3.pk_supplierclass where  not exists (select cgeneralhid from (select distinct pob.cgeneralhid cgeneralhid from ic_purchasein_b pob left join bd_material mat on mat.pk_material = pob.cmaterialvid and nvl(mat.dr,0)=0 left join ic_purchasein_h poh on poh.cgeneralhid = pob.cgeneralhid and nvl(poh.dr,0)=0 where  nvl(pob.dr,0)=0 and substr(mat.code,0,4) = '0915' and poh.PK_ORG='0001A110000000001V70'  AND substr(poh.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and poh.fbillflag=3) po  where po.cgeneralhid = A.cgeneralhid) and  A.PK_ORG='0001A110000000001V70' and A.dr!=1 and A.cwarehouseid not in('1001A1100000000T5S5Z','1001A11000000003CYSY') AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillflag=3 and A5.nqtorigprice  is not null";
                }
                else
                {
                    string delstr = "delete from Rdrecord01 where id in(select ID from Rdrecord01 where zt != 1 )";
                    string delstr2 = "delete from Rdrecords01 where id in(select ID from Rdrecord01 where zt != 1 )";
                    SqlHelperForBulk.ExecuteNonQuerys(delstr2);
                    SqlHelperForBulk.ExecuteNonQuerys(delstr);
                    string str = "select id from Rdrecord01";
                    DataSet ds = SqlHelperForBulk.ExecuteDataset(connectionString, CommandType.Text, str);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        updateCount = ds.Tables[0].Rows.Count;
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            strbu.Append(dr["id"].ToString() + ",");
                        }
                        strbu = strbu.Remove(strbu.Length - 1, 1);
                        String[] ids = strbu.ToString().Split(',');
                        strGetOracleSQLIn = getOracleSQLIn(ids, "A.cgeneralhid");
                        //获取采购入库头数据
                        sql = " select distinct A.cgeneralhid ID, A.vbillcode code, A.dbilldate ddate, A2.code cwhcode, A3.code cvencode,A4.code cvenclass,A3.name cvenname,A1.CODE cdepcode,CASE WHEN A.freplenishflag='N' THEN 0 ELSE 1 END AS isRed,A.vnote remark, A.modifiedtime ts FROM ic_purchasein_h A left join ic_purchasein_b A5 on A5.cgeneralhid = A.cgeneralhid  left join org_dept A1 on A1.pk_dept = A.cdptid left join bd_stordoc A2 on A2.pk_stordoc = A.cwarehouseid left join bd_supplier A3 on A3.pk_supplier = A.cvendorid left join bd_supplierclass A4 on A4.pk_supplierclass=A3.pk_supplierclass where  not exists (select cgeneralhid from (select distinct pob.cgeneralhid cgeneralhid from ic_purchasein_b pob left join bd_material mat on mat.pk_material = pob.cmaterialvid and nvl(mat.dr,0)=0 left join ic_purchasein_h poh on poh.cgeneralhid = pob.cgeneralhid and nvl(poh.dr,0)=0 where  nvl(pob.dr,0)=0 and substr(mat.code,0,4) = '0915' and poh.PK_ORG='0001A110000000001V70'  AND substr(poh.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and poh.fbillflag=3) po  where po.cgeneralhid = A.cgeneralhid) and  A.PK_ORG='0001A110000000001V70' and A.dr!=1 and A.cwarehouseid not in('1001A1100000000T5S5Z','1001A11000000003CYSY') AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillflag=3 and " + strGetOracleSQLIn + " and A5.nqtorigprice  is not null";
                    }
                    else
                    {
                        updateCount = 0;
                        sql = " select distinct A.cgeneralhid ID, A.vbillcode code, A.dbilldate ddate, A2.code cwhcode, A3.code cvencode,A4.code cvenclass,A3.name cvenname,A1.CODE cdepcode,CASE WHEN A.freplenishflag='N' THEN 0 ELSE 1 END AS isRed,A.vnote remark, A.modifiedtime ts FROM ic_purchasein_h A left join ic_purchasein_b A5 on A5.cgeneralhid = A.cgeneralhid  left join org_dept A1 on A1.pk_dept = A.cdptid left join bd_stordoc A2 on A2.pk_stordoc = A.cwarehouseid left join bd_supplier A3 on A3.pk_supplier = A.cvendorid left join bd_supplierclass A4 on A4.pk_supplierclass=A3.pk_supplierclass where  not exists (select cgeneralhid from (select distinct pob.cgeneralhid cgeneralhid from ic_purchasein_b pob left join bd_material mat on mat.pk_material = pob.cmaterialvid and nvl(mat.dr,0)=0 left join ic_purchasein_h poh on poh.cgeneralhid = pob.cgeneralhid and nvl(poh.dr,0)=0 where  nvl(pob.dr,0)=0 and substr(mat.code,0,4) = '0915' and poh.PK_ORG='0001A110000000001V70'  AND substr(poh.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and poh.fbillflag=3) po  where po.cgeneralhid = A.cgeneralhid) and  A.PK_ORG='0001A110000000001V70' and A.dr!=1 and A.cwarehouseid not in('1001A1100000000T5S5Z','1001A11000000003CYSY') AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillflag=3 and A5.nqtorigprice  is not null";
                    }
                }
                //获取采购入库头数据
                DataSet PurchaseIn = OracleHelper.ExecuteDataset(sql);


                //判断当前表是否存在 1存在 0 不存在
                tableExist = "if object_id( 'Rdrecord01') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    //获取采购入库行数据
                    sql = "SELECT A.cgeneralhid ID,A1.cgeneralbid autoid,A1.crowno doclineno,A2.code cinvcode,A2.name cinvname,A3.code cinvclass,A3.name cinvclassname,A2.materialspec cinvstd,A4.code cinvUnit,NVL(A1.nassistnum ,0) qty, A1.ntaxrate itaxrate, A1.nqtorigtaxprice iOriTaxCost, A1.nqtorigprice iOriCost,A1.norigtaxmny ioriSum, A1.ntax iOriTaxPrice, A1.norigmny iOriMoney, A1.ntaxmny isum,A1.nmny iMoney, A1.ntax iTaxPrice, A1.nqtprice iUnitCost,A1.vnotebody remark FROM ic_purchasein_h A left join ic_purchasein_b A1 on A1.cgeneralhid = A.cgeneralhid and A1.dr!=1 left join bd_material A2 on A1.cmaterialvid = A2.pk_material left join bd_marbasclass A3 ON A2.PK_MARBASCLASS = A3.PK_MARBASCLASS left join bd_measdoc A4 on A2.PK_MEASDOC = A4.pk_measdoc where A.PK_ORG = '0001A110000000001V70' AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillflag = 3 and substr(A2.code,0,4) != '0915' and A.cwarehouseid  not in('1001A1100000000T5S5Z','1001A11000000003CYSY') and A1.nqtorigprice  is not null";
                }
                else
                {
                    if (updateCount > 0)
                    {
                        //获取采购入库行数据
                        sql = "SELECT A.cgeneralhid ID,A1.cgeneralbid autoid,A1.crowno doclineno,A2.code cinvcode,A2.name cinvname,A3.code cinvclass,A3.name cinvclassname,A2.materialspec cinvstd,A4.code cinvUnit,NVL(A1.nassistnum ,0) qty, A1.ntaxrate itaxrate, A1.nqtorigtaxprice iOriTaxCost, A1.nqtorigprice iOriCost,A1.norigtaxmny ioriSum, A1.ntax iOriTaxPrice, A1.norigmny iOriMoney, A1.ntaxmny isum,A1.nmny iMoney, A1.ntax iTaxPrice, A1.nqtprice iUnitCost,A1.vnotebody remark FROM ic_purchasein_h A left join ic_purchasein_b A1 on A1.cgeneralhid = A.cgeneralhid and A1.dr!=1 left join bd_material A2 on A1.cmaterialvid = A2.pk_material left join bd_marbasclass A3 ON A2.PK_MARBASCLASS = A3.PK_MARBASCLASS left join bd_measdoc A4 on A2.PK_MEASDOC = A4.pk_measdoc where A.PK_ORG = '0001A110000000001V70' AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillflag = 3 and substr(A2.code,0,4) != '0915' and A.cwarehouseid not in('1001A1100000000T5S5Z','1001A11000000003CYSY') and " + strGetOracleSQLIn + " and A1.nqtorigprice  is not null";
                    }
                    else
                    {
                        sql = "SELECT A.cgeneralhid ID,A1.cgeneralbid autoid,A1.crowno doclineno,A2.code cinvcode,A2.name cinvname,A3.code cinvclass,A3.name cinvclassname,A2.materialspec cinvstd,A4.code cinvUnit,NVL(A1.nassistnum ,0) qty, A1.ntaxrate itaxrate, A1.nqtorigtaxprice iOriTaxCost, A1.nqtorigprice iOriCost,A1.norigtaxmny ioriSum, A1.ntax iOriTaxPrice, A1.norigmny iOriMoney, A1.ntaxmny isum,A1.nmny iMoney, A1.ntax iTaxPrice, A1.nqtprice iUnitCost,A1.vnotebody remark FROM ic_purchasein_h A left join ic_purchasein_b A1 on A1.cgeneralhid = A.cgeneralhid and A1.dr!=1 left join bd_material A2 on A1.cmaterialvid = A2.pk_material left join bd_marbasclass A3 ON A2.PK_MARBASCLASS = A3.PK_MARBASCLASS left join bd_measdoc A4 on A2.PK_MEASDOC = A4.pk_measdoc where A.PK_ORG = '0001A110000000001V70' AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillflag = 3 and substr(A2.code,0,4) != '0915' and A.cwarehouseid  not in('1001A1100000000T5S5Z','1001A11000000003CYSY') and A1.nqtorigprice  is not null";
                    }
                }

                //获取采购入库行数据
                DataSet PurchaseInLine = OracleHelper.ExecuteDataset(sql);

                //判断当前表是否存在 1存在 0 不存在
                tableExist = "if object_id( 'Rdrecord01') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    createSql = "create table Rdrecord01(ID nvarchar(30) primary key not null,code nvarchar(50),ddate nvarchar(20),cwhcode nvarchar(50),cvencode nvarchar(50),cvenclass nvarchar(50),cvenname nvarchar(50),cdepcode nvarchar(50),isRed bit,remark nvarchar(200),ts nvarchar(50),crdcode nvarchar(50) default '101',cptcode nvarchar(10) default '01',zt bit default 0,memo text)";
                    SqlHelperForBulk.ExecuteNonQuery(createSql);
                    //StringBuilder str = DataSetToArrayList.DataSetToArrayLists(PurchaseIn, "Rdrecord01");
                    //SqlHelperForBulk.ExecuteNonQuery(str.ToString());

                    SqlBulkCopyHelperForTest.ImportTempTableDataIndex(PurchaseIn, "Rdrecord01");
                    msg = "采购入库表插入成功";
                }
                else
                {
                    //StringBuilder str = DataSetToArrayList.DataSetToArrayLists(PurchaseIn, "Rdrecord01");
                    if (JudgeDs(PurchaseIn))
                    {
                        //SqlHelperForBulk.ExecuteNonQuery(str.ToString());
                        SqlBulkCopyHelperForTest.ImportTempTableDataIndex(PurchaseIn, "Rdrecord01");
                        msg = "采购入库表更新成功";
                    }
                    else
                    {
                        msg = "采购入库表暂无可更新数据";
                    }
                }
                tableExist = "if object_id( 'Rdrecords01') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    createSql = "create table Rdrecords01(ID nvarchar(30),autoid nvarchar(30)  primary key not null,doclineno bigint,cinvcode nvarchar(50),cinvname nvarchar(500),cinvclass nvarchar(500),cinvclassname nvarchar(500),cinvstd nvarchar(500),cinvUnit nvarchar(500),qty decimal(28, 8),itaxrate decimal(28, 8),iOriTaxCost decimal(28, 8),iOriCost decimal(28, 8),ioriSum decimal(28, 8),iOriTaxPrice decimal(28, 8),iOriMoney decimal(28, 8),isum decimal(28, 8),iMoney decimal(28, 8),iTaxPrice decimal(28, 8),iUnitCost decimal(28, 8),remark nvarchar(200))";
                    SqlHelperForBulk.ExecuteNonQuery(createSql);

                    //StringBuilder strs = DataSetToArrayList.DataSetToArrayLists(PurchaseInLine, "Rdrecords01");
                    //SqlHelperForBulk.ExecuteNonQuery(strs.ToString());

                    SqlBulkCopyHelperForTest.ImportTempTableDataIndex(PurchaseInLine, "Rdrecords01");
                    msg = "采购入库表行插入成功";
                }
                else
                {
                    //StringBuilder strs = DataSetToArrayList.DataSetToArrayLists(PurchaseInLine, "Rdrecords01");
                    if (JudgeDs(PurchaseInLine))
                    {
                        //SqlHelperForBulk.ExecuteNonQuery(strs.ToString());
                        SqlBulkCopyHelperForTest.ImportTempTableDataIndex(PurchaseInLine, "Rdrecords01");
                        msg = "采购入库表行更新成功";
                    }
                    else
                    {
                        msg = "采购入库表暂无可更新数据";
                    }
                }
                //GetU8SVApiUrlApi("cgrkapi");
                result = msg;
            }
            catch (Exception e)
            {

                result = "采购入库表错误：" + e.Message;
            }
            return result;
        }


        /// <summary>
        /// 从nc获取材料出库数据插入到sql
        /// 创建人：lvhe
        /// 创建时间：2019年11月28日 15:09:23
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        private string GetMaterialToSql()
        {
            string result = "";
            string createSql = "";
            string tableExist = "";
            int existResult = 0;
            string msg = "";
            string sql = "";
            int updateCount = 0;
            StringBuilder strbu = new StringBuilder();
            string strGetOracleSQLIn = "";
            try
            {
                //判断当前表是否存在 1存在 0 不存在
                tableExist = "if object_id('RdRecord11') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);
                if (existResult == 0)
                {
                    //获取材料出库头数据
                    sql = "select A.cgeneralhid ID,A.vbillcode code,A.dbilldate ddate,A1.Code cwhcode,A2.pk_billtypecode crdcode, A3.code cdepcode, A.vnote remark,CASE WHEN A.ntotalnum > 0 THEN 0 ELSE 1 END AS isRed,A.modifiedtime ts from ic_material_h A left join bd_stordoc A1 on A.cwarehouseid = A1.Pk_Stordoc left join bd_billtype A2 on A.ctrantypeid = A2.pk_billtypeid left join org_dept A3 on A3.PK_DEPT = A.cdptid where  not exists (select cgeneralhid from (select distinct pob.cgeneralhid cgeneralhid from ic_material_b pob left join bd_material mat on mat.pk_material = pob.cmaterialvid and nvl(mat.dr,0)=0 left join ic_material_h poh on poh.cgeneralhid = pob.cgeneralhid and nvl(poh.dr,0)=0 where  nvl(pob.dr,0)=0 and substr(mat.code,0,4) = '0915' and poh.PK_ORG='0001A110000000001V70'  AND substr(poh.taudittime,0,10) between '" + startTime + "' and '" + endTime + "' and poh.fbillflag=3) po  where po.cgeneralhid = A.cgeneralhid) and  A.PK_ORG='0001A110000000001V70'  AND substr(A.taudittime,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillflag=3 and A.cwarehouseid not in('1001A1100000000T5S5Z','1001A11000000003CYSY')";
                }
                else
                {
                    string delstr = "delete from RdRecord11 where id in(select ID from RdRecord11 where zt != 1 )";
                    string delstr2 = "delete from RdRecords11 where id in(select ID from RdRecord11 where zt != 1 )";
                    SqlHelperForBulk.ExecuteNonQuerys(delstr2);
                    SqlHelperForBulk.ExecuteNonQuerys(delstr);
                    string str = "select id from RdRecord11";
                    DataSet ds = SqlHelperForBulk.ExecuteDataset(connectionString, CommandType.Text, str);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        updateCount = ds.Tables[0].Rows.Count;
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            strbu.Append(dr["id"].ToString() + ",");
                        }
                        strbu = strbu.Remove(strbu.Length - 1, 1);
                        String[] ids = strbu.ToString().Split(',');
                        strGetOracleSQLIn = getOracleSQLIn(ids, "A.cgeneralhid");
                        //获取采购入库头数据
                        sql = "select A.cgeneralhid ID,A.vbillcode code,A.dbilldate ddate,A1.Code cwhcode,A2.pk_billtypecode crdcode, A3.code cdepcode, A.vnote remark,CASE WHEN A.ntotalnum > 0 THEN 0 ELSE 1 END AS isRed,A.modifiedtime ts from ic_material_h A left join bd_stordoc A1 on A.cwarehouseid = A1.Pk_Stordoc left join bd_billtype A2 on A.ctrantypeid = A2.pk_billtypeid left join org_dept A3 on A3.PK_DEPT = A.cdptid where  not exists (select cgeneralhid from (select distinct pob.cgeneralhid cgeneralhid from ic_material_b pob left join bd_material mat on mat.pk_material = pob.cmaterialvid and nvl(mat.dr,0)=0 left join ic_material_h poh on poh.cgeneralhid = pob.cgeneralhid and nvl(poh.dr,0)=0 where  nvl(pob.dr,0)=0 and substr(mat.code,0,4) = '0915' and poh.PK_ORG='0001A110000000001V70'  AND substr(poh.taudittime,0,10) between '" + startTime + "' and '" + endTime + "' and poh.fbillflag=3) po  where po.cgeneralhid = A.cgeneralhid) and  A.PK_ORG='0001A110000000001V70'  AND substr(A.taudittime,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillflag=3 and A.cwarehouseid not in('1001A1100000000T5S5Z','1001A11000000003CYSY') and " + strGetOracleSQLIn + "";
                    }
                    else
                    {
                        updateCount = 0;
                        sql = "select A.cgeneralhid ID,A.vbillcode code,A.dbilldate ddate,A1.Code cwhcode,A2.pk_billtypecode crdcode, A3.code cdepcode, A.vnote remark,CASE WHEN A.ntotalnum > 0 THEN 0 ELSE 1 END AS isRed,A.modifiedtime ts from ic_material_h A left join bd_stordoc A1 on A.cwarehouseid = A1.Pk_Stordoc left join bd_billtype A2 on A.ctrantypeid = A2.pk_billtypeid left join org_dept A3 on A3.PK_DEPT = A.cdptid where  not exists (select cgeneralhid from (select distinct pob.cgeneralhid cgeneralhid from ic_material_b pob left join bd_material mat on mat.pk_material = pob.cmaterialvid and nvl(mat.dr,0)=0 left join ic_material_h poh on poh.cgeneralhid = pob.cgeneralhid and nvl(poh.dr,0)=0 where  nvl(pob.dr,0)=0 and substr(mat.code,0,4) = '0915' and poh.PK_ORG='0001A110000000001V70'  AND substr(poh.taudittime,0,10) between '" + startTime + "' and '" + endTime + "' and poh.fbillflag=3) po  where po.cgeneralhid = A.cgeneralhid) and  A.PK_ORG='0001A110000000001V70'  AND substr(A.taudittime,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillflag=3 and A.cwarehouseid not in('1001A1100000000T5S5Z','1001A11000000003CYSY')";
                    }
                }
                DataSet Material = OracleHelper.ExecuteDataset(sql);

                tableExist = "if object_id( 'RdRecords11') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    //获取材料出库行数据
                    sql = "select A1.cgeneralhid ID,A1.cgeneralbid autoid,A1.crowno doclineno,A2.code cinvcode,A2.Name cinvname,A3.code cinvclass,A3.Name cinvclassname,A2.materialspec cinvstd,A4.code cinvUnit,NVL(A1.nassistnum ,0) qty, A1.vnotebody remark from ic_material_h A left join ic_material_b A1 on A.cgeneralhid = A1.cgeneralhid and A1.DR != 1 left join bd_material A2 on A1.cmaterialvid = A2.pk_material left join bd_marbasclass A3 on A2.pk_marbasclass=A3.pk_marbasclass left join bd_measdoc A4 on A4.pk_measdoc=A2.pk_measdoc where A.PK_ORG = '0001A110000000001V70' and A.DR != 1 AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "'  and A.fbillflag = 3 and substr(A2.code,0,4) != '0915' and A.cwarehouseid not in('1001A1100000000T5S5Z','1001A11000000003CYSY')";
                }
                else
                {
                    if (updateCount > 0)
                    {
                        sql = "select A1.cgeneralhid ID,A1.cgeneralbid autoid,A1.crowno doclineno,A2.code cinvcode,A2.Name cinvname,A3.code cinvclass,A3.Name cinvclassname,A2.materialspec cinvstd,A4.code cinvUnit,NVL(A1.nassistnum ,0) qty, A1.vnotebody remark from ic_material_h A left join ic_material_b A1 on A.cgeneralhid = A1.cgeneralhid and A1.DR != 1 left join bd_material A2 on A1.cmaterialvid = A2.pk_material left join bd_marbasclass A3 on A2.pk_marbasclass=A3.pk_marbasclass left join bd_measdoc A4 on A4.pk_measdoc=A2.pk_measdoc where A.PK_ORG = '0001A110000000001V70' and A.DR != 1 AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "'  and A.fbillflag = 3 and substr(A2.code,0,4) != '0915' and A.cwarehouseid not in('1001A1100000000T5S5Z','1001A11000000003CYSY') and " + strGetOracleSQLIn + "";
                    }
                    else
                    {
                        sql = "select A1.cgeneralhid ID,A1.cgeneralbid autoid,A1.crowno doclineno,A2.code cinvcode,A2.Name cinvname,A3.code cinvclass,A3.Name cinvclassname,A2.materialspec cinvstd,A4.code cinvUnit,NVL(A1.nassistnum ,0) qty, A1.vnotebody remark from ic_material_h A left join ic_material_b A1 on A.cgeneralhid = A1.cgeneralhid and A1.DR != 1 left join bd_material A2 on A1.cmaterialvid = A2.pk_material left join bd_marbasclass A3 on A2.pk_marbasclass=A3.pk_marbasclass left join bd_measdoc A4 on A4.pk_measdoc=A2.pk_measdoc where A.PK_ORG = '0001A110000000001V70' and A.DR != 1 AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "'  and A.fbillflag = 3 and substr(A2.code,0,4) != '0915' and A.cwarehouseid not in('1001A1100000000T5S5Z','1001A11000000003CYSY')";
                    }
                }
                DataSet MaterialLine = OracleHelper.ExecuteDataset(sql);

                //判断当前表是否存在 1存在 0 不存在
                tableExist = "if object_id('RdRecord11') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    createSql = "create table RdRecord11(ID nvarchar(30) primary key not null,code nvarchar(50),ddate nvarchar(20),cwhcode nvarchar(100),crdcode nvarchar(20),cdepcode nvarchar(50),remark nvarchar(100),isRed bit default 0,ts nvarchar(50),zt bit default 0, memo text)";
                    SqlHelperForBulk.ExecuteNonQuery(createSql);
                    //StringBuilder str = DataSetToArrayList.DataSetToArrayLists(Material, "RdRecord11");
                    //SqlHelperForBulk.ExecuteNonQuery(str.ToString());

                    SqlBulkCopyHelperForTest.ImportTempTableDataIndex(Material, "RdRecord11");
                    msg = "材料出库表插入成功";
                }
                else
                {
                    //StringBuilder str = DataSetToArrayList.DataSetToArrayLists(Material, "RdRecord11");
                    if (JudgeDs(Material))
                    {
                        //SqlHelperForBulk.ExecuteNonQuery(str.ToString());

                        SqlBulkCopyHelperForTest.ImportTempTableDataIndex(Material, "RdRecord11");
                        msg = "材料出库表更新成功";
                    }
                    else
                    {
                        msg = "材料出库表暂无可更新数据";
                    }
                }
                tableExist = "if object_id( 'RdRecords11') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    createSql = "create table RdRecords11(ID nvarchar(30),autoid nvarchar(30)  primary key not null,doclineno nvarchar(50),cinvcode nvarchar(max),cinvname nvarchar(max),cinvclass nvarchar(max),cinvclassname nvarchar(max),cinvstd nvarchar(max),cinvUnit nvarchar(max),qty decimal(28, 8),remark nvarchar(max))";
                    SqlHelperForBulk.ExecuteNonQuery(createSql);
                    //StringBuilder strs = DataSetToArrayList.DataSetToArrayLists(MaterialLine, "RdRecords11");
                    //SqlHelperForBulk.ExecuteNonQuery(strs.ToString());

                    SqlBulkCopyHelperForTest.ImportTempTableDataIndex(MaterialLine, "RdRecords11");
                    msg = "材料出库表行插入成功";
                }
                else
                {
                    StringBuilder strs = DataSetToArrayList.DataSetToArrayLists(MaterialLine, "RdRecords11");
                    if (JudgeDs(MaterialLine))
                    {
                        //SqlHelperForBulk.ExecuteNonQuery(strs.ToString());

                        SqlBulkCopyHelperForTest.ImportTempTableDataIndex(MaterialLine, "RdRecords11");
                        msg = "材料出库表行更新成功";
                    }
                    else
                    {
                        msg = "材料出库表暂无可更新数据";
                    }
                }
                //GetU8SVApiUrlApi("clckapi");
                result = msg;
            }
            catch (Exception e)
            {

                result = "材料出库表错误：" + e.Message;
            }
            return result;

        }


        /// <summary>
        /// 从nc获取产成品入库单数据插入到sql
        /// 创建人：lvhe
        /// 创建时间：2019年12月14日 17:57:01
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        private string GetFinprodInToSql()
        {
            string result = "";
            string createSql = "";
            string tableExist = "";
            int existResult = 0;
            string msg = "";
            string sql = "";
            int updateCount = 0;
            StringBuilder strbu = new StringBuilder();
            string strGetOracleSQLIn = "";
            try
            {
                //判断当前表是否存在 1存在 0 不存在
                tableExist = "if object_id( 'RdRecord10') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    //获取产成品入库头数据
                    sql = "SELECT A.cgeneralhid ID, A.vbillcode code, A.dbilldate ddate, A2.code cwhcode, A3.pk_billtypecode crdcode,A1.CODE cdepcode, A.vnote remark,CASE WHEN A.ntotalnum > 0 THEN 0 ELSE 1 END AS isRed, A.modifiedtime ts FROM ic_finprodin_h A left join org_dept_v A1 on A1.pk_vid = A.cdptvid left join bd_stordoc A2 on A2.pk_stordoc = A.cwarehouseid left join bd_billtype A3 on A3.pk_billtypeid = A.ctrantypeid where  not exists (select cgeneralhid from (select distinct pob.cgeneralhid cgeneralhid from ic_finprodin_b pob left join bd_material mat on mat.pk_material = pob.cmaterialvid and nvl(mat.dr,0)=0 left join ic_finprodin_h poh on poh.cgeneralhid = pob.cgeneralhid and nvl(poh.dr,0)=0 where  nvl(pob.dr,0)=0 and substr(mat.code,0,4) = '0915' and poh.PK_ORG='0001A110000000001V70'  AND substr(poh.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and poh.fbillflag=3) po  where po.cgeneralhid = A.cgeneralhid) and  A.PK_ORG='0001A110000000001V70'  AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillflag=3 and A.cwarehouseid not in('1001A1100000000T5S5Z','1001A11000000003CYSY')";
                }
                else
                {
                    string delstr = "delete from RdRecord10 where id in(select ID from RdRecord10 where zt != 1 )";
                    string delstr2 = "delete from RdRecords10 where id in(select ID from RdRecord10 where zt != 1 )";
                    SqlHelperForBulk.ExecuteNonQuerys(delstr2);
                    SqlHelperForBulk.ExecuteNonQuerys(delstr);
                    string str = "select id from RdRecord10";
                    DataSet ds = SqlHelperForBulk.ExecuteDataset(connectionString, CommandType.Text, str);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        updateCount = ds.Tables[0].Rows.Count;
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            strbu.Append(dr["id"].ToString() + ",");
                        }
                        strbu = strbu.Remove(strbu.Length - 1, 1);
                        String[] ids = strbu.ToString().Split(',');
                        strGetOracleSQLIn = getOracleSQLIn(ids, "A.cgeneralhid");
                        //获取采购入库头数据
                        sql = "SELECT A.cgeneralhid ID, A.vbillcode code, A.dbilldate ddate, A2.code cwhcode, A3.pk_billtypecode crdcode,A1.CODE cdepcode, A.vnote remark,CASE WHEN A.ntotalnum > 0 THEN 0 ELSE 1 END AS isRed, A.modifiedtime ts FROM ic_finprodin_h A left join org_dept A1 on A1.pk_dept = A.cdptid left join bd_stordoc A2 on A2.pk_stordoc = A.cwarehouseid left join bd_billtype A3 on A3.pk_billtypeid = A.ctrantypeid where  not exists (select cgeneralhid from (select distinct pob.cgeneralhid cgeneralhid from ic_finprodin_b pob left join bd_material mat on mat.pk_material = pob.cmaterialvid and nvl(mat.dr,0)=0 left join ic_finprodin_h poh on poh.cgeneralhid = pob.cgeneralhid and nvl(poh.dr,0)=0 where  nvl(pob.dr,0)=0 and substr(mat.code,0,4) = '0915' and poh.PK_ORG='0001A110000000001V70'  AND substr(poh.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and poh.fbillflag=3) po  where po.cgeneralhid = A.cgeneralhid) and  A.PK_ORG='0001A110000000001V70'  AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillflag=3 and A.cwarehouseid not in('1001A1100000000T5S5Z','1001A11000000003CYSY') and " + strGetOracleSQLIn + "";
                    }
                    else
                    {
                        updateCount = 0;
                        sql = "SELECT A.cgeneralhid ID, A.vbillcode code, A.dbilldate ddate, A2.code cwhcode, A3.pk_billtypecode crdcode,A1.CODE cdepcode, A.vnote remark,CASE WHEN A.ntotalnum > 0 THEN 0 ELSE 1 END AS isRed, A.modifiedtime ts FROM ic_finprodin_h A left join org_dept A1 on A1.pk_dept = A.cdptid left join bd_stordoc A2 on A2.pk_stordoc = A.cwarehouseid left join bd_billtype A3 on A3.pk_billtypeid = A.ctrantypeid where  not exists (select cgeneralhid from (select distinct pob.cgeneralhid cgeneralhid from ic_finprodin_b pob left join bd_material mat on mat.pk_material = pob.cmaterialvid and nvl(mat.dr,0)=0 left join ic_finprodin_h poh on poh.cgeneralhid = pob.cgeneralhid and nvl(poh.dr,0)=0 where  nvl(pob.dr,0)=0 and substr(mat.code,0,4) = '0915' and poh.PK_ORG='0001A110000000001V70'  AND substr(poh.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and poh.fbillflag=3) po  where po.cgeneralhid = A.cgeneralhid) and  A.PK_ORG='0001A110000000001V70'  AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillflag=3 and A.cwarehouseid not in('1001A1100000000T5S5Z','1001A11000000003CYSY')";
                    }
                }
                DataSet FinprodIn = OracleHelper.ExecuteDataset(sql);

                tableExist = "if object_id( 'RdRecords10') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    //获取产成品入库行数据
                    sql = "SELECT A.cgeneralhid ID,A1.cgeneralbid autoid,A1.crowno doclineno,A2.code cinvcode,A2.name cinvname, A3.code cinvclass,A3.name cinvclassname, A2.materialspec cinvstd, A4.code cinvUnit,NVL(A1.nassistnum ,0) qty, A1.vnotebody remark FROM ic_finprodin_h A left join ic_finprodin_b A1 on A1.cgeneralhid = A.cgeneralhid and A1.dr != 1 left join bd_material A2 on A1.cmaterialvid = A2.pk_material left join bd_marbasclass A3 ON A2.PK_MARBASCLASS = A3.PK_MARBASCLASS left join bd_measdoc A4 on A2.PK_MEASDOC = A4.pk_measdoc  where A.PK_ORG = '0001A110000000001V70' AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillflag = 3 and substr(A2.code,0,4) != '0915' and A.cwarehouseid not in('1001A1100000000T5S5Z','1001A11000000003CYSY')";
                }
                else
                {
                    if (updateCount > 0)
                    {
                        sql = "SELECT A.cgeneralhid ID,A1.cgeneralbid autoid,A1.crowno doclineno,A2.code cinvcode,A2.name cinvname, A3.code cinvclass,A3.name cinvclassname, A2.materialspec cinvstd, A4.code cinvUnit,NVL(A1.nassistnum ,0) qty, A1.vnotebody remark FROM ic_finprodin_h A left join ic_finprodin_b A1 on A1.cgeneralhid = A.cgeneralhid and A1.dr != 1 left join bd_material A2 on A1.cmaterialvid = A2.pk_material left join bd_marbasclass A3 ON A2.PK_MARBASCLASS = A3.PK_MARBASCLASS left join bd_measdoc A4 on A2.PK_MEASDOC = A4.pk_measdoc  where A.PK_ORG = '0001A110000000001V70' AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillflag = 3 and substr(A2.code,0,4) != '0915' and A.cwarehouseid not in('1001A1100000000T5S5Z','1001A11000000003CYSY') and " + strGetOracleSQLIn + "";
                    }
                    else
                    {
                        sql = "SELECT A.cgeneralhid ID,A1.cgeneralbid autoid,A1.crowno doclineno,A2.code cinvcode,A2.name cinvname, A3.code cinvclass,A3.name cinvclassname, A2.materialspec cinvstd, A4.code cinvUnit,NVL(A1.nassistnum ,0) qty, A1.vnotebody remark FROM ic_finprodin_h A left join ic_finprodin_b A1 on A1.cgeneralhid = A.cgeneralhid and A1.dr != 1 left join bd_material A2 on A1.cmaterialvid = A2.pk_material left join bd_marbasclass A3 ON A2.PK_MARBASCLASS = A3.PK_MARBASCLASS left join bd_measdoc A4 on A2.PK_MEASDOC = A4.pk_measdoc  where A.PK_ORG = '0001A110000000001V70' AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillflag = 3 and substr(A2.code,0,4) != '0915' and A.cwarehouseid not in('1001A1100000000T5S5Z','1001A11000000003CYSY')";
                    }
                }
                DataSet FinprodInLine = OracleHelper.ExecuteDataset(sql);

                //判断当前表是否存在 1存在 0 不存在
                tableExist = "if object_id( 'RdRecord10') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    createSql = "create table RdRecord10(ID nvarchar(30) primary key not null,code nvarchar(50),ddate nvarchar(20),cwhcode nvarchar(100),crdcode nvarchar(20),cdepcode nvarchar(50),remark nvarchar(100),isRed bit default 0,ts nvarchar(50),zt bit default 0,memo text)";
                    SqlHelperForBulk.ExecuteNonQuery(createSql);
                    //StringBuilder str = DataSetToArrayList.DataSetToArrayLists(FinprodIn, "RdRecord10");
                    //SqlHelperForBulk.ExecuteNonQuery(str.ToString());

                    SqlBulkCopyHelperForTest.ImportTempTableDataIndex(FinprodIn, "RdRecord10");
                    msg = "产成品入库表插入成功";
                }
                else
                {
                    //StringBuilder str = DataSetToArrayList.DataSetToArrayLists(FinprodIn, "RdRecord10");
                    if (JudgeDs(FinprodIn))
                    {
                        //SqlHelperForBulk.ExecuteNonQuery(str.ToString());
                        SqlBulkCopyHelperForTest.ImportTempTableDataIndex(FinprodIn, "RdRecord10");
                        msg = "产成品入库表更新成功";
                    }
                    else
                    {
                        msg = "产成品入库表暂无可更新数据";
                    }
                }
                tableExist = "if object_id( 'RdRecords10') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    createSql = "create table RdRecords10(ID nvarchar(30),autoid nvarchar(30)  primary key not null,doclineno nvarchar(50),cinvcode nvarchar(50),cinvname nvarchar(500),cinvclass nvarchar(500),cinvclassname nvarchar(500),cinvstd nvarchar(500),cinvUnit  nvarchar(500),qty decimal(28,8),remark nvarchar(100))";
                    SqlHelperForBulk.ExecuteNonQuery(createSql);
                    //StringBuilder strs = DataSetToArrayList.DataSetToArrayLists(FinprodInLine, "RdRecords10");
                    //SqlHelperForBulk.ExecuteNonQuery(strs.ToString());

                    SqlBulkCopyHelperForTest.ImportTempTableDataIndex(FinprodInLine, "RdRecords10");
                    msg = "产成品入库表行插入成功";
                }
                else
                {
                    //StringBuilder strs = DataSetToArrayList.DataSetToArrayLists(FinprodInLine, "RdRecords10");
                    if (JudgeDs(FinprodInLine))
                    {
                        //SqlHelperForBulk.ExecuteNonQuery(strs.ToString());

                        SqlBulkCopyHelperForTest.ImportTempTableDataIndex(FinprodInLine, "RdRecords10");
                        msg = "产成品入库表行更新成功";
                    }
                    else
                    {
                        msg = "产成品入库表暂无可更新数据";
                    }
                }
                //GetU8SVApiUrlApi("ccprkapi");
                result = msg;
            }
            catch (Exception e)
            {

                result = "产成品入库错误：" + e.Message;
            }
            return result;

        }


        /// <summary>
        /// 从nc获取其他入库数据插入到sql
        /// 创建人：lvhe
        /// 创建时间：2019年12月9日 21:53:53
        /// </summary>
        /// <returns></returns>
        private string GetIAi4billToSql()
        {
            string result = "";
            string createSql = "";
            string tableExist = "";
            int existResult = 0;
            string msg = "";
            string sql = "";
            int updateCount = 0;
            StringBuilder strbu = new StringBuilder();
            string strGetOracleSQLIn = "";
            try
            {
                //获取其他入库头数据
                //判断当前表是否存在 1存在 0 不存在
                tableExist = "if object_id('RdRecord08') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    sql = "SELECT distinct A.cgeneralhid ID, A.vbillcode code, A.dbilldate ddate, A2.code cwhcode,A3.pk_billtypecode crdcode,A1.CODE cdepcode, A1.name cdepname,A.vnote remark,A.modifiedtime ts FROM ic_generalin_h A left join org_dept A1 on A1.pk_dept = A.cdptid left join bd_stordoc A2 on A2.pk_stordoc = A.cwarehouseid left join bd_billtype A3 on A3.pk_billtypeid = A.ctrantypeid INNER join ic_generalin_b A4 on A.cgeneralhid=A4.cgeneralhid LEFT JOIN bd_stordoc A5 on A5.pk_stordoc=A.cwarehouseid left join bd_material mat on mat.pk_material = A4.cmaterialvid where substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A2.CODE NOT IN('PPG','STJGC') AND A.vtrantypecode not IN ('4A-02','4A-06') AND MAT.CODE NOT LIKE '0915%' and A.PK_ORG='0001A110000000001V70' AND A.DR=0";
                }
                else
                {
                    string delstr = "delete from RdRecord08 where id in(select ID from RdRecord08 where zt != 1 )";
                    string delstr2 = "delete from RdRecords08 where id in(select ID from RdRecord08 where zt != 1 )";
                    SqlHelperForBulk.ExecuteNonQuerys(delstr2);
                    SqlHelperForBulk.ExecuteNonQuerys(delstr);
                    string str = "select id from RdRecord08";
                    DataSet ds = SqlHelperForBulk.ExecuteDataset(connectionString, CommandType.Text, str);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        updateCount = ds.Tables[0].Rows.Count;
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            strbu.Append(dr["id"].ToString() + ",");
                        }
                        strbu = strbu.Remove(strbu.Length - 1, 1);
                        String[] ids = strbu.ToString().Split(',');
                        strGetOracleSQLIn = getOracleSQLIn(ids, "A.cgeneralhid");
                        //获取采购入库头数据
                        sql = "SELECT distinct A.cgeneralhid ID, A.vbillcode code, A.dbilldate ddate, A2.code cwhcode,A3.pk_billtypecode crdcode,A1.CODE cdepcode, A1.name cdepname,A.vnote remark,A.modifiedtime ts FROM ic_generalin_h A left join org_dept A1 on A1.pk_dept = A.cdptid left join bd_stordoc A2 on A2.pk_stordoc = A.cwarehouseid left join bd_billtype A3 on A3.pk_billtypeid = A.ctrantypeid INNER join ic_generalin_b A4 on A.cgeneralhid=A4.cgeneralhid LEFT JOIN bd_stordoc A5 on A5.pk_stordoc=A.cwarehouseid left join bd_material mat on mat.pk_material = A4.cmaterialvid where substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A2.CODE NOT IN('PPG','STJGC') AND A.vtrantypecode not IN ('4A-02','4A-06') AND MAT.CODE NOT LIKE '0915%' and A.PK_ORG='0001A110000000001V70' AND A.DR=0  and " + strGetOracleSQLIn + "";

                    }
                    else
                    {
                        updateCount = 0;
                        sql = "SELECT distinct A.cgeneralhid ID, A.vbillcode code, A.dbilldate ddate, A2.code cwhcode,A3.pk_billtypecode crdcode,A1.CODE cdepcode, A1.name cdepname,A.vnote remark,A.modifiedtime ts FROM ic_generalin_h A left join org_dept A1 on A1.pk_dept = A.cdptid left join bd_stordoc A2 on A2.pk_stordoc = A.cwarehouseid left join bd_billtype A3 on A3.pk_billtypeid = A.ctrantypeid INNER join ic_generalin_b A4 on A.cgeneralhid=A4.cgeneralhid LEFT JOIN bd_stordoc A5 on A5.pk_stordoc=A.cwarehouseid left join bd_material mat on mat.pk_material = A4.cmaterialvid where substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A2.CODE NOT IN('PPG','STJGC') AND A.vtrantypecode not IN ('4A-02','4A-06') AND MAT.CODE NOT LIKE '0915%' and A.PK_ORG='0001A110000000001V70' AND A.DR=0";
                    }
                }
                DataSet IAi4bill = OracleHelper.ExecuteDataset(sql);

                tableExist = "if object_id( 'RdRecords08') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    //获取其他入库行数据
                    sql = "SELECT A.cgeneralhid ID,A1.cgeneralbid autoid,A1.crowno doclineno,A2.code cinvcode, A2.name cinvname, A3.code cinvclass,A3.name cinvclassname,A2.materialspec cinvstd, A4.code cinvUnit,NVL(A1.nnum,0) qty FROM ic_generalin_h A left join ic_generalin_b A1 on A1.cgeneralhid = A.cgeneralhid and A1.dr != 1 left join bd_material A2 on A1.cmaterialvid = A2.pk_material left join bd_marbasclass A3 on A3.pk_marbasclass = A2.pk_marbasclass left join bd_measdoc A4 on A4.pk_measdoc = A2.pk_measdoc left join bd_billtype A5 on A5.pk_billtypeid = A.ctrantypeid left join bd_stordoc A6 on A6.pk_stordoc = A.cwarehouseid where  (A.vtrantypecode not IN ('4A-02','4A-06')) AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A2.CODE NOT LIKE '0915%' and A6.CODE NOT IN('PPG','STJGC') and A.PK_ORG='0001A110000000001V70'";
                }
                else
                {
                    if (updateCount > 0)
                    {
                        sql = "SELECT A.cgeneralhid ID,A1.cgeneralbid autoid,A1.crowno doclineno,A2.code cinvcode, A2.name cinvname, A3.code cinvclass,A3.name cinvclassname,A2.materialspec cinvstd, A4.code cinvUnit,NVL(A1.nnum,0) qty FROM ic_generalin_h A left join ic_generalin_b A1 on A1.cgeneralhid = A.cgeneralhid and A1.dr != 1 left join bd_material A2 on A1.cmaterialvid = A2.pk_material left join bd_marbasclass A3 on A3.pk_marbasclass = A2.pk_marbasclass left join bd_measdoc A4 on A4.pk_measdoc = A2.pk_measdoc left join bd_billtype A5 on A5.pk_billtypeid = A.ctrantypeid left join bd_stordoc A6 on A6.pk_stordoc = A.cwarehouseid where  (A.vtrantypecode not IN ('4A-02','4A-06')) AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A2.CODE NOT LIKE '0915%' and A6.CODE NOT IN('PPG','STJGC') and A.PK_ORG='0001A110000000001V70' and " + strGetOracleSQLIn + "";
                    }
                    else
                    {
                        sql = "SELECT A.cgeneralhid ID,A1.cgeneralbid autoid,A1.crowno doclineno,A2.code cinvcode, A2.name cinvname, A3.code cinvclass,A3.name cinvclassname,A2.materialspec cinvstd, A4.code cinvUnit,NVL(A1.nnum,0) qty FROM ic_generalin_h A left join ic_generalin_b A1 on A1.cgeneralhid = A.cgeneralhid and A1.dr != 1 left join bd_material A2 on A1.cmaterialvid = A2.pk_material left join bd_marbasclass A3 on A3.pk_marbasclass = A2.pk_marbasclass left join bd_measdoc A4 on A4.pk_measdoc = A2.pk_measdoc left join bd_billtype A5 on A5.pk_billtypeid = A.ctrantypeid left join bd_stordoc A6 on A6.pk_stordoc = A.cwarehouseid where  (A.vtrantypecode not IN ('4A-02','4A-06')) AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A2.CODE NOT LIKE '0915%' and A6.CODE NOT IN('PPG','STJGC') and A.PK_ORG='0001A110000000001V70'";
                    }
                }
                DataSet IAi4billLine = OracleHelper.ExecuteDataset(sql);

                //判断当前表是否存在 1存在 0 不存在
                tableExist = "if object_id('RdRecord08') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    createSql = "create table RdRecord08(ID nvarchar(30) primary key not null,code nvarchar(50),ddate nvarchar(20),cwhcode nvarchar(100),crdcode nvarchar(20),cdepcode nvarchar(50),cdepname nvarchar(50),remark nvarchar(100),ts nvarchar(50),isRed bit default 0,zt bit default 0,memo text)";
                    SqlHelperForBulk.ExecuteNonQuery(createSql);
                    //StringBuilder str = DataSetToArrayList.DataSetToArrayLists(IAi4bill, "RdRecord08");
                    //SqlHelperForBulk.ExecuteNonQuery(str.ToString());

                    SqlBulkCopyHelperForTest.ImportTempTableDataIndex(IAi4bill, "RdRecord08");
                    msg = "其他入库表插入成功";
                }
                else
                {
                    //StringBuilder str = DataSetToArrayList.DataSetToArrayLists(IAi4bill, "RdRecord08");
                    if (JudgeDs(IAi4bill))
                    {
                        //SqlHelperForBulk.ExecuteNonQuery(str.ToString());
                        SqlBulkCopyHelperForTest.ImportTempTableDataIndex(IAi4bill, "RdRecord08");
                        msg = "其他入库表更新成功";
                    }
                    else
                    {
                        msg = "其他入库表暂无可更新数据";
                    }
                }
                tableExist = "if object_id('RdRecords08') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    createSql = "create table RdRecords08(ID nvarchar(30),autoid nvarchar(30)  primary key not null,doclineno nvarchar(50),cinvcode nvarchar(50),cinvname nvarchar(500),cinvclass nvarchar(500),cinvclassname nvarchar(500),cinvstd nvarchar(500),cinvUnit  nvarchar(500),qty decimal(28,8),remark nvarchar(100))";
                    SqlHelperForBulk.ExecuteNonQuery(createSql);
                    //StringBuilder strs = DataSetToArrayList.DataSetToArrayLists(IAi4billLine, "RdRecords08");
                    //SqlHelperForBulk.ExecuteNonQuery(strs.ToString());

                    SqlBulkCopyHelperForTest.ImportTempTableDataIndex(IAi4billLine, "RdRecords08");
                    msg = "其他入库表行插入成功";
                }
                else
                {
                    //StringBuilder strs = DataSetToArrayList.DataSetToArrayLists(IAi4billLine, "RdRecords08");
                    if (JudgeDs(IAi4billLine))
                    {
                        //SqlHelperForBulk.ExecuteNonQuery(strs.ToString());

                        SqlBulkCopyHelperForTest.ImportTempTableDataIndex(IAi4billLine, "RdRecords08");
                        msg = "其他入库表行更新成功";
                    }
                    else
                    {
                        msg = "其他入库表暂无可更新数据";
                    }
                }
                //GetU8SVApiUrlApi("qtrkdapi");
                result = msg;
            }
            catch (Exception e)
            {

                result = "其他入库表错误：" + e.Message;
            }
            return result;

        }


        /// <summary>
        /// 从nc获取其他出库数据插入到sql
        /// 创建人：lvhe
        /// 创建时间：2019年12月9日 21:53:53
        /// </summary>
        /// <returns></returns>
        private string GetIAi7billToSql()
        {
            string result = "";
            string createSql = "";
            string tableExist = "";
            int existResult = 0;
            string msg = "";
            string sql = "";
            int updateCount = 0;
            StringBuilder strbu = new StringBuilder();
            string strGetOracleSQLIn = "";
            try
            {
                //判断当前表是否存在 1存在 0 不存在
                tableExist = "if object_id('RdRecord09') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    //获取其他出库头数据
                    sql = "SELECT distinct A.cgeneralhid ID, A.vbillcode code, A.dbilldate ddate, A2.code cwhcode,A3.pk_billtypecode crdcode,A1.CODE cdepcode, A1.name cdepname,A.vnote remark,A.modifiedtime ts FROM ic_generalout_h A left join org_dept A1 on A1.pk_dept = A.cdptid left join bd_stordoc A2 on A2.pk_stordoc = A.cwarehouseid left join bd_billtype A3 on A3.pk_billtypeid = A.ctrantypeid INNER join ic_generalout_b A4 on A.cgeneralhid=A4.cgeneralhid LEFT JOIN bd_stordoc A5 on A5.pk_stordoc=A.cwarehouseid left join bd_material mat on mat.pk_material = A4.cmaterialvid where substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A2.CODE NOT IN('PPG','STJGC') AND A.vtrantypecode not IN ('4I-02','4I-06') AND MAT.CODE NOT LIKE '0915%' and A.PK_ORG='0001A110000000001V70' AND A.DR=0";
                }
                else
                {
                    string delstr = "delete from RdRecord09 where id in(select ID from RdRecord09 where zt != 1 )";
                    string delstr2 = "delete from RdRecords09 where id in(select ID from RdRecord09 where zt != 1 )";
                    SqlHelperForBulk.ExecuteNonQuerys(delstr2);
                    SqlHelperForBulk.ExecuteNonQuerys(delstr);
                    string str = "select id from RdRecord09";
                    DataSet ds = SqlHelperForBulk.ExecuteDataset(connectionString, CommandType.Text, str);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        updateCount = ds.Tables[0].Rows.Count;
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            strbu.Append(dr["id"].ToString() + ",");
                        }
                        strbu = strbu.Remove(strbu.Length - 1, 1);
                        String[] ids = strbu.ToString().Split(',');
                        strGetOracleSQLIn = getOracleSQLIn(ids, "A.cgeneralhid");
                        //获取采购入库头数据
                        sql = "SELECT distinct A.cgeneralhid ID, A.vbillcode code, A.dbilldate ddate, A2.code cwhcode,A3.pk_billtypecode crdcode,A1.CODE cdepcode, A1.name cdepname,A.vnote remark,A.modifiedtime ts FROM ic_generalout_h A left join org_dept A1 on A1.pk_dept = A.cdptid left join bd_stordoc A2 on A2.pk_stordoc = A.cwarehouseid left join bd_billtype A3 on A3.pk_billtypeid = A.ctrantypeid INNER join ic_generalout_b A4 on A.cgeneralhid=A4.cgeneralhid LEFT JOIN bd_stordoc A5 on A5.pk_stordoc=A.cwarehouseid left join bd_material mat on mat.pk_material = A4.cmaterialvid where substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A2.CODE NOT IN('PPG','STJGC') AND A.vtrantypecode not IN ('4I-02','4I-06') AND MAT.CODE NOT LIKE '0915%' and A.PK_ORG='0001A110000000001V70' AND A.DR=0 and " + strGetOracleSQLIn + "";

                    }
                    else
                    {
                        updateCount = 0;
                        sql = "SELECT distinct A.cgeneralhid ID, A.vbillcode code, A.dbilldate ddate, A2.code cwhcode,A3.pk_billtypecode crdcode,A1.CODE cdepcode, A1.name cdepname,A.vnote remark,A.modifiedtime ts FROM ic_generalout_h A left join org_dept A1 on A1.pk_dept = A.cdptid left join bd_stordoc A2 on A2.pk_stordoc = A.cwarehouseid left join bd_billtype A3 on A3.pk_billtypeid = A.ctrantypeid INNER join ic_generalout_b A4 on A.cgeneralhid=A4.cgeneralhid LEFT JOIN bd_stordoc A5 on A5.pk_stordoc=A.cwarehouseid left join bd_material mat on mat.pk_material = A4.cmaterialvid where substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A2.CODE NOT IN('PPG','STJGC') AND A.vtrantypecode not IN ('4I-02','4I-06') AND MAT.CODE NOT LIKE '0915%' and A.PK_ORG='0001A110000000001V70' AND A.DR=0";
                    }
                }
                DataSet IAi7bill = OracleHelper.ExecuteDataset(sql);

                tableExist = "if object_id( 'RdRecords09') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    //获取其他出库行数据
                    sql = "SELECT A.cgeneralhid ID,A1.cgeneralbid autoid,A1.crowno doclineno,A2.code cinvcode, A2.name cinvname, A3.code cinvclass,A3.name cinvclassname, A2.materialspec cinvstd, A4.code cinvUnit,NVL(A1.nassistnum,0) qty FROM ic_generalout_h A left join ic_generalout_b A1 on A1.cgeneralhid = A.cgeneralhid and A1.dr != 1 left join bd_material A2 on A1.cmaterialvid = A2.pk_material left join bd_marbasclass A3 on A3.pk_marbasclass = A2.pk_marbasclass left join bd_measdoc A4 on A4.pk_measdoc = A2.pk_measdoc left join bd_billtype A5 on A5.pk_billtypeid = A.ctrantypeid left join bd_stordoc A6 on A6.pk_stordoc = A.cwarehouseid where  (A.vtrantypecode not IN ('4I-02','4I-06')) AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A2.CODE NOT LIKE '0915%' and A6.CODE NOT IN('PPG','STJGC') and A.PK_ORG='0001A110000000001V70'";
                }
                else
                {
                    if (updateCount > 0)
                    {
                        sql = "SELECT A.cgeneralhid ID,A1.cgeneralbid autoid,A1.crowno doclineno,A2.code cinvcode, A2.name cinvname, A3.code cinvclass,A3.name cinvclassname, A2.materialspec cinvstd, A4.code cinvUnit,NVL(A1.nassistnum,0) qty FROM ic_generalout_h A left join ic_generalout_b A1 on A1.cgeneralhid = A.cgeneralhid and A1.dr != 1 left join bd_material A2 on A1.cmaterialvid = A2.pk_material left join bd_marbasclass A3 on A3.pk_marbasclass = A2.pk_marbasclass left join bd_measdoc A4 on A4.pk_measdoc = A2.pk_measdoc left join bd_billtype A5 on A5.pk_billtypeid = A.ctrantypeid left join bd_stordoc A6 on A6.pk_stordoc = A.cwarehouseid where  (A.vtrantypecode not IN ('4I-02','4I-06')) AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A2.CODE NOT LIKE '0915%' and A6.CODE NOT IN('PPG','STJGC') and A.PK_ORG='0001A110000000001V70' and " + strGetOracleSQLIn + "";
                    }
                    else
                    {
                        sql = "SELECT A.cgeneralhid ID,A1.cgeneralbid autoid,A1.crowno doclineno,A2.code cinvcode, A2.name cinvname, A3.code cinvclass,A3.name cinvclassname, A2.materialspec cinvstd, A4.code cinvUnit,NVL(A1.nassistnum,0) qty FROM ic_generalout_h A left join ic_generalout_b A1 on A1.cgeneralhid = A.cgeneralhid and A1.dr != 1 left join bd_material A2 on A1.cmaterialvid = A2.pk_material left join bd_marbasclass A3 on A3.pk_marbasclass = A2.pk_marbasclass left join bd_measdoc A4 on A4.pk_measdoc = A2.pk_measdoc left join bd_billtype A5 on A5.pk_billtypeid = A.ctrantypeid left join bd_stordoc A6 on A6.pk_stordoc = A.cwarehouseid where  (A.vtrantypecode not IN ('4I-02','4I-06')) AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A2.CODE NOT LIKE '0915%' and A6.CODE NOT IN('PPG','STJGC') and A.PK_ORG='0001A110000000001V70'";
                    }
                }
                DataSet IAi7billLine = OracleHelper.ExecuteDataset(sql);

                //判断当前表是否存在 1存在 0 不存在
                tableExist = "if object_id('RdRecord09') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    createSql = "create table RdRecord09(ID nvarchar(30) primary key not null,code nvarchar(50),ddate nvarchar(20),cwhcode nvarchar(100),crdcode nvarchar(20),cdepcode nvarchar(50),cdepname nvarchar(50),remark nvarchar(100),ts nvarchar(50),isRed bit default 0,zt bit default 0,memo text)";
                    SqlHelperForBulk.ExecuteNonQuery(createSql);
                    //StringBuilder str = DataSetToArrayList.DataSetToArrayLists(IAi7bill, "RdRecord09");
                    //SqlHelperForBulk.ExecuteNonQuery(str.ToString());

                    SqlBulkCopyHelperForTest.ImportTempTableDataIndex(IAi7bill, "RdRecord09");
                    msg = "其他出库表插入成功";
                }
                else
                {
                    //StringBuilder str = DataSetToArrayList.DataSetToArrayLists(IAi7bill, "RdRecord09");
                    if (JudgeDs(IAi7bill))
                    {
                        //SqlHelperForBulk.ExecuteNonQuery(str.ToString());

                        SqlBulkCopyHelperForTest.ImportTempTableDataIndex(IAi7bill, "RdRecord09");
                        msg = "其他出库表更新成功";
                    }
                    else
                    {
                        msg = "其他出库表暂无可更新数据";
                    }
                }
                tableExist = "if object_id( 'RdRecords09') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    createSql = "create table RdRecords09(ID nvarchar(30),autoid nvarchar(30)  primary key not null,doclineno nvarchar(50),cinvcode nvarchar(50),cinvname nvarchar(500),cinvclass nvarchar(500),cinvclassname nvarchar(500),cinvstd nvarchar(500),cinvUnit  nvarchar(500),qty decimal(28,8),remark nvarchar(100))";
                    SqlHelperForBulk.ExecuteNonQuery(createSql);
                    //StringBuilder strs = DataSetToArrayList.DataSetToArrayLists(IAi7billLine, "RdRecords09");
                    //SqlHelperForBulk.ExecuteNonQuery(strs.ToString());

                    SqlBulkCopyHelperForTest.ImportTempTableDataIndex(IAi7billLine, "RdRecords09");
                    msg = "其他出库表行插入成功";
                }
                else
                {
                    //StringBuilder strs = DataSetToArrayList.DataSetToArrayLists(IAi7billLine, "RdRecords09");
                    if (JudgeDs(IAi7billLine))
                    {
                        //SqlHelperForBulk.ExecuteNonQuery(strs.ToString());

                        SqlBulkCopyHelperForTest.ImportTempTableDataIndex(IAi7billLine, "RdRecords09");
                        msg = "其他出库表行更新成功";
                    }
                    else
                    {
                        msg = "其他出库表暂无可更新数据";
                    }
                }
                //GetU8SVApiUrlApi("qtckdapi");
                result = msg;
            }
            catch (Exception e)
            {

                result = "其他出库表错误：" + e.Message;
            }
            return result;

        }


        /// <summary>
        /// 从形态转换单单数据插入到sql
        /// 创建人：lvhe
        /// 创建时间：2019-12-19 23:56:56
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        private string GetIcTransformHToSql()
        {
            string result = "";
            string createSql = "";
            string tableExist = "";
            int existResult = 0;
            string msg = "";
            string sql = "";
            int updateCount = 0;
            StringBuilder strbu = new StringBuilder();
            string strGetOracleSQLIn = "";
            try
            {
                //判断当前表是否存在 1存在 0 不存在
                tableExist = "if object_id( 'AssemVouch') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    //获取形态转换单单头数据
                    sql = " SELECT A.cspecialhid ID, A.vbillcode code, A.dbilldate ddate,A1.code cdepcode,A.vnote remark,0 isRed,A.modifiedtime ts FROM ic_transform_h A left join org_dept_v A1 on A1.pk_vid = A.cdptvid where  not exists (select cspecialhid from (select distinct pob.cspecialhid cspecialhid from ic_transform_b pob left join bd_material mat on mat.pk_material = pob.cmaterialvid and nvl(mat.dr,0)=0 left join ic_transform_h poh on poh.cspecialhid = pob.cspecialhid and nvl(poh.dr,0)=0 where  nvl(pob.dr,0)=0 and pob.cbodywarehouseid not in('1001A1100000000T5S5Z','1001A11000000003CYSY') and substr(mat.code,0,4) = '0915' and poh.PK_ORG='0001A110000000001V70'  AND substr(poh.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "') po  where po.cspecialhid = A.cspecialhid) and A.dr != 1  AND  A.PK_ORG='0001A110000000001V70'  AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' ";
                }
                else
                {
                    string delstr = "delete from AssemVouch where id in(select ID from AssemVouch where zt != 1 )";
                    string delstr2 = "delete from AssemVouchs where id in(select ID from AssemVouch where zt != 1 )";
                    SqlHelperForBulk.ExecuteNonQuerys(delstr2);
                    SqlHelperForBulk.ExecuteNonQuerys(delstr);
                    string str = "select id from AssemVouch";
                    DataSet ds = SqlHelperForBulk.ExecuteDataset(connectionString, CommandType.Text, str);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        updateCount = ds.Tables[0].Rows.Count;
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            strbu.Append(dr["id"].ToString() + ",");
                        }
                        strbu = strbu.Remove(strbu.Length - 1, 1);
                        String[] ids = strbu.ToString().Split(',');
                        strGetOracleSQLIn = getOracleSQLIn(ids, "A.cspecialhid");
                        //获取形态转换单单头数据
                        sql = " SELECT A.cspecialhid ID, A.vbillcode code, A.dbilldate ddate,A1.code cdepcode,A.vnote remark,0 isRed,A.modifiedtime ts FROM ic_transform_h A left join org_dept_v A1 on A1.pk_vid = A.cdptvid where  not exists (select cspecialhid from (select distinct pob.cspecialhid cspecialhid from ic_transform_b pob left join bd_material mat on mat.pk_material = pob.cmaterialvid and nvl(mat.dr,0)=0 left join ic_transform_h poh on poh.cspecialhid = pob.cspecialhid and nvl(poh.dr,0)=0 where  nvl(pob.dr,0)=0 and pob.cbodywarehouseid not in('1001A1100000000T5S5Z','1001A11000000003CYSY') and substr(mat.code,0,4) = '0915' and poh.PK_ORG='0001A110000000001V70'  AND substr(poh.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "') po  where po.cspecialhid = A.cspecialhid) and A.dr != 1  AND A.PK_ORG='0001A110000000001V70'  AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and " + strGetOracleSQLIn + "";
                    }
                    else
                    {
                        updateCount = 0;
                        sql = " SELECT A.cspecialhid ID, A.vbillcode code, A.dbilldate ddate,A1.code cdepcode,A.vnote remark,0 isRed,A.modifiedtime ts FROM ic_transform_h A left join org_dept_v A1 on A1.pk_vid = A.cdptvid where  not exists (select cspecialhid from (select distinct pob.cspecialhid cspecialhid from ic_transform_b pob left join bd_material mat on mat.pk_material = pob.cmaterialvid and nvl(mat.dr,0)=0 left join ic_transform_h poh on poh.cspecialhid = pob.cspecialhid and nvl(poh.dr,0)=0 where  nvl(pob.dr,0)=0 and pob.cbodywarehouseid not in('1001A1100000000T5S5Z','1001A11000000003CYSY') and substr(mat.code,0,4) = '0915' and poh.PK_ORG='0001A110000000001V70'  AND substr(poh.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "') po  where po.cspecialhid = A.cspecialhid) and A.dr != 1  AND A.PK_ORG='0001A110000000001V70'  AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' ";
                    }
                }
                DataSet IcTransformH = OracleHelper.ExecuteDataset(sql);

                tableExist = "if object_id( 'AssemVouchs') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    //获取形态转换单单行数据
                    sql = " SELECT A.cspecialhid ID,A1.cspecialbid autoid,CASE WHEN A1.fbillrowflag = 2 THEN '转换前' WHEN A1.fbillrowflag = 3 THEN '转换后' ELSE '' END AS bavtype,CASE WHEN (A1.cbeforebid is not null and A1.cbeforebid ='~') THEN A1.cspecialbid  ELSE A1.cbeforebid END AS igroupno,A5.code cwhcode,A1.crowno doclineno,A2.code cinvcode, A2.name cinvname, A3.code cinvclass, A2.materialspec cinvstd,A4.code cinvUnit,NVL(A1.nassistnum,0) qty, A1.vnotebody remark FROM ic_transform_h A left join ic_transform_b A1 on A1.cspecialhid = A.cspecialhid and A1.dr != 1 left join bd_material A2 on A1.cmaterialvid = A2.pk_material left join bd_marbasclass A3 ON A2.PK_MARBASCLASS = A3.PK_MARBASCLASS left join bd_measdoc A4 on A2.PK_MEASDOC = A4.pk_measdoc left join bd_stordoc A5 on A5.pk_stordoc = A1.cbodywarehouseid where A.PK_ORG = '0001A110000000001V70' AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and substr(A2.code,0,4) != '0915' and A5.code not in('1001A1100000000T5S5Z','1001A11000000003CYSY')";
                }
                else
                {
                    if (updateCount > 0)
                    {
                        sql = " SELECT A.cspecialhid ID,A1.cspecialbid autoid,CASE WHEN A1.fbillrowflag = 2 THEN '转换前' WHEN A1.fbillrowflag = 3 THEN '转换后' ELSE '' END AS bavtype,CASE WHEN (A1.cbeforebid is not null and A1.cbeforebid ='~') THEN A1.cspecialbid  ELSE A1.cbeforebid END AS igroupno,A5.code cwhcode,A1.crowno doclineno,A2.code cinvcode, A2.name cinvname, A3.code cinvclass, A2.materialspec cinvstd,A4.code cinvUnit,NVL(A1.nassistnum,0) qty, A1.vnotebody remark FROM ic_transform_h A left join ic_transform_b A1 on A1.cspecialhid = A.cspecialhid and A1.dr != 1 left join bd_material A2 on A1.cmaterialvid = A2.pk_material left join bd_marbasclass A3 ON A2.PK_MARBASCLASS = A3.PK_MARBASCLASS left join bd_measdoc A4 on A2.PK_MEASDOC = A4.pk_measdoc left join bd_stordoc A5 on A5.pk_stordoc = A1.cbodywarehouseid where A.PK_ORG = '0001A110000000001V70' AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and substr(A2.code,0,4) != '0915' and A5.code not in('1001A1100000000T5S5Z','1001A11000000003CYSY') and " + strGetOracleSQLIn + "";
                    }
                    else
                    {
                        sql = " SELECT A.cspecialhid ID,A1.cspecialbid autoid,CASE WHEN A1.fbillrowflag = 2 THEN '转换前' WHEN A1.fbillrowflag = 3 THEN '转换后' ELSE '' END AS bavtype,CASE WHEN (A1.cbeforebid is not null and A1.cbeforebid ='~') THEN A1.cspecialbid  ELSE A1.cbeforebid END AS igroupno,A5.code cwhcode,A1.crowno doclineno,A2.code cinvcode, A2.name cinvname, A3.code cinvclass, A2.materialspec cinvstd,A4.code cinvUnit,NVL(A1.nassistnum,0) qty, A1.vnotebody remark FROM ic_transform_h A left join ic_transform_b A1 on A1.cspecialhid = A.cspecialhid and A1.dr != 1 left join bd_material A2 on A1.cmaterialvid = A2.pk_material left join bd_marbasclass A3 ON A2.PK_MARBASCLASS = A3.PK_MARBASCLASS left join bd_measdoc A4 on A2.PK_MEASDOC = A4.pk_measdoc left join bd_stordoc A5 on A5.pk_stordoc = A1.cbodywarehouseid where A.PK_ORG = '0001A110000000001V70' AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and substr(A2.code,0,4) != '0915' and A5.code not in('1001A1100000000T5S5Z','1001A11000000003CYSY')";
                    }
                }
                DataSet IcTransformHLine = OracleHelper.ExecuteDataset(sql);

                //判断当前表是否存在 1存在 0 不存在
                tableExist = "if object_id( 'AssemVouch') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    createSql = "create table AssemVouch(ID nvarchar(30),code nvarchar(50),ddate nvarchar(20),cdepcode nvarchar(50),remark nvarchar(100),isRed bit default 0,ts nvarchar(50),zt bit default 0,memo text)";
                    SqlHelperForBulk.ExecuteNonQuery(createSql);
                    if (IcTransformH.Tables.Count > 0 && IcTransformH.Tables[0].Rows.Count > 0)
                    {
                        //StringBuilder str = DataSetToArrayList.DataSetToArrayLists(IcTransformH, "AssemVouch");
                        //SqlHelperForBulk.ExecuteNonQuery(str.ToString());


                        SqlBulkCopyHelperForTest.ImportTempTableDataIndex(IcTransformH, "AssemVouch");
                        msg = "形态转换表插入成功";
                    }
                    else
                    {
                        msg = "形态转换表上月无数据";
                    }
                }
                else
                {
                    if (IcTransformH.Tables.Count > 0 && IcTransformH.Tables[0].Rows.Count > 0)
                    {
                        //StringBuilder str = DataSetToArrayList.DataSetToArrayLists(IcTransformH, "AssemVouch");
                        if (JudgeDs(IcTransformH))
                        {
                            //SqlHelperForBulk.ExecuteNonQuery(str.ToString());

                            SqlBulkCopyHelperForTest.ImportTempTableDataIndex(IcTransformH, "AssemVouch");
                            msg = "形态转换表更新成功";
                        }
                        else
                        {
                            msg = "形态转换表暂无可更新数据";
                        }
                    }
                    else
                    {
                        msg = "形态转换表上月无数据";
                    }
                }
                tableExist = "if object_id( 'AssemVouchs') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    createSql = "create table AssemVouchs(ID nvarchar(30),autoid nvarchar(30),bavtype nvarchar(50),igroupno nvarchar(50),cwhcode nvarchar(50),doclineno nvarchar(50),cinvcode nvarchar(50),cinvname nvarchar(500),cinvclass nvarchar(500),cinvstd nvarchar(500),cinvUnit  nvarchar(500),qty decimal(28,8),remark nvarchar(100))";
                    SqlHelperForBulk.ExecuteNonQuery(createSql);
                    if (IcTransformHLine.Tables.Count > 0 && IcTransformHLine.Tables[0].Rows.Count > 0)
                    {
                        //StringBuilder strs = DataSetToArrayList.DataSetToArrayLists(IcTransformHLine, "AssemVouchs");
                        if (JudgeDs(IcTransformHLine))
                        {
                            //SqlHelperForBulk.ExecuteNonQuery(strs.ToString());

                            SqlBulkCopyHelperForTest.ImportTempTableDataIndex(IcTransformHLine, "AssemVouchs");
                            msg = "形态转换表行插入成功";
                        }
                        else
                        {
                            msg = "形态转换表暂无可更新数据";
                        }
                    }
                    else
                    {
                        msg = "形态转换表上月无数据";
                    }
                }
                else
                {
                    if (IcTransformHLine.Tables.Count > 0 && IcTransformHLine.Tables[0].Rows.Count > 0)
                    {
                        //StringBuilder strs = DataSetToArrayList.DataSetToArrayLists(IcTransformHLine, "AssemVouchs");
                        if (JudgeDs(IcTransformHLine))
                        {
                            //SqlHelperForBulk.ExecuteNonQuery(strs.ToString());

                            SqlBulkCopyHelperForTest.ImportTempTableDataIndex(IcTransformHLine, "AssemVouchs");
                            msg = "形态转换表行更新成功";
                        }
                        else
                        {
                            msg = "形态转换表暂无可更新数据";
                        }
                    }
                    else
                    {
                        msg = "形态转换表上月无数据";
                    }
                }
                //GetU8SVApiUrlApi("xtzhdapi");
                result = msg;
            }
            catch (Exception e)
            {

                result = "形态转换表错误：" + e.Message;
            }
            return result;

        }


        /// <summary>
        /// 从nc调拨单  NC 转库单数据插入到sql
        /// 创建人：lvhe
        /// 创建时间：2019-12-19 23:14:10
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        private string GetIcWhstransHToSql()
        {
            string result = "";
            string createSql = "";
            string tableExist = "";
            int existResult = 0;
            string msg = "";
            string sql = "";
            int updateCount = 0;
            StringBuilder strbu = new StringBuilder();
            string strGetOracleSQLIn = "";
            try
            {
                //判断当前表是否存在 1存在 0 不存在
                tableExist = "if object_id( 'TransVouch') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    //获取转库单头数据
                    sql = "SELECT A.cspecialhid ID, A.vbillcode code, A.dbilldate ddate, A1.code cowhcode,A2.code ciwhcode,A3.code codepcode, A4.code cidepcode, A.vnote remark, CASE WHEN A.ntotalnum > 0 THEN 0 ELSE 1 END AS isRed,A.modifiedtime ts FROM ic_whstrans_h A left join bd_stordoc A1 on A1.pk_stordoc = A.cwarehouseid left join bd_stordoc A2 on A2.pk_stordoc = A.cotherwhid left join org_dept_v A3 on A3.pk_vid = A.cdptvid left join org_dept_v A4 on A4.pk_vid = A.cotherdptvid where  not exists (select cspecialhid from (select distinct pob.cspecialhid cspecialhid from ic_whstrans_b pob left join bd_material mat on mat.pk_material = pob.cmaterialvid and nvl(mat.dr,0)=0 left join ic_whstrans_h poh on poh.cspecialhid = pob.cspecialhid and nvl(poh.dr,0)=0 where  nvl(pob.dr,0)=0 and substr(mat.code,0,4) = '0915' and poh.PK_ORG='0001A110000000001V70'  AND substr(poh.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and poh.fbillflag=4) po  where po.cspecialhid = A.cspecialhid) and  A.PK_ORG='0001A110000000001V70'  AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillflag=4 and A.cwarehouseid not in('1001A1100000000T5S5Z','1001A11000000003CYSY')  and A.cotherwhid  not in('1001A1100000000T5S5Z','1001A11000000003CYSY')";
                }
                else
                {
                    string delstr = "delete from TransVouch where id in(select ID from TransVouch where zt != 1 )";
                    string delstr2 = "delete from TransVouchs where id in(select ID from TransVouch where zt != 1 )";
                    SqlHelperForBulk.ExecuteNonQuerys(delstr2);
                    SqlHelperForBulk.ExecuteNonQuerys(delstr);
                    string str = "select id from TransVouch";
                    DataSet ds = SqlHelperForBulk.ExecuteDataset(connectionString, CommandType.Text, str);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        updateCount = ds.Tables[0].Rows.Count;
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            strbu.Append(dr["id"].ToString() + ",");
                        }
                        strbu = strbu.Remove(strbu.Length - 1, 1);
                        String[] ids = strbu.ToString().Split(',');
                        strGetOracleSQLIn = getOracleSQLIn(ids, "A.cspecialhid");
                        //获取采购入库头数据
                        sql = "SELECT A.cspecialhid ID, A.vbillcode code, A.dbilldate ddate, A1.code cowhcode,A2.code ciwhcode,A3.code codepcode, A4.code cidepcode, A.vnote remark, CASE WHEN A.ntotalnum > 0 THEN 0 ELSE 1 END AS isRed,A.modifiedtime ts FROM ic_whstrans_h A left join bd_stordoc A1 on A1.pk_stordoc = A.cwarehouseid left join bd_stordoc A2 on A2.pk_stordoc = A.cotherwhid left join org_dept_v A3 on A3.pk_vid = A.cdptvid left join org_dept_v A4 on A4.pk_vid = A.cotherdptvid where  not exists (select cspecialhid from (select distinct pob.cspecialhid cspecialhid from ic_whstrans_b pob left join bd_material mat on mat.pk_material = pob.cmaterialvid and nvl(mat.dr,0)=0 left join ic_whstrans_h poh on poh.cspecialhid = pob.cspecialhid and nvl(poh.dr,0)=0 where  nvl(pob.dr,0)=0 and substr(mat.code,0,4) = '0915' and poh.PK_ORG='0001A110000000001V70'  AND substr(poh.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and poh.fbillflag=4) po  where po.cspecialhid = A.cspecialhid) and  A.PK_ORG='0001A110000000001V70'  AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillflag=4 and A.cwarehouseid not in('1001A1100000000T5S5Z','1001A11000000003CYSY')  and A.cotherwhid  not in('1001A1100000000T5S5Z','1001A11000000003CYSY') and " + strGetOracleSQLIn + "";
                    }
                    else
                    {
                        updateCount = 0;
                        sql = "SELECT A.cspecialhid ID, A.vbillcode code, A.dbilldate ddate, A1.code cowhcode,A2.code ciwhcode,A3.code codepcode, A4.code cidepcode, A.vnote remark, CASE WHEN A.ntotalnum > 0 THEN 0 ELSE 1 END AS isRed,A.modifiedtime ts FROM ic_whstrans_h A left join bd_stordoc A1 on A1.pk_stordoc = A.cwarehouseid left join bd_stordoc A2 on A2.pk_stordoc = A.cotherwhid left join org_dept_v A3 on A3.pk_vid = A.cdptvid left join org_dept_v A4 on A4.pk_vid = A.cotherdptvid where  not exists (select cspecialhid from (select distinct pob.cspecialhid cspecialhid from ic_whstrans_b pob left join bd_material mat on mat.pk_material = pob.cmaterialvid and nvl(mat.dr,0)=0 left join ic_whstrans_h poh on poh.cspecialhid = pob.cspecialhid and nvl(poh.dr,0)=0 where  nvl(pob.dr,0)=0 and substr(mat.code,0,4) = '0915' and poh.PK_ORG='0001A110000000001V70'  AND substr(poh.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and poh.fbillflag=4) po  where po.cspecialhid = A.cspecialhid) and  A.PK_ORG='0001A110000000001V70'  AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillflag=4 and A.cwarehouseid not in('1001A1100000000T5S5Z','1001A11000000003CYSY')  and A.cotherwhid  not in('1001A1100000000T5S5Z','1001A11000000003CYSY')";
                    }
                }
                DataSet IcWhstransH = OracleHelper.ExecuteDataset(sql);

                tableExist = "if object_id( 'TransVouchs') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    //获取转库单行数据
                    sql = "SELECT A.cspecialhid ID,A1.cspecialbid autoid,A1.crowno doclineno,A2.code cinvcode, A2.name cinvname, A3.code cinvclass,A3.name cinvclassname, A2.materialspec cinvstd,A4.code cinvUnit,NVL(A1.nassistnum ,0) qty, A1.vnotebody remark FROM ic_whstrans_h A left join ic_whstrans_b A1 on A1.cspecialhid = A.cspecialhid and A1.dr != 1 left join bd_material A2 on A1.cmaterialvid = A2.pk_material left join bd_marbasclass A3 ON A2.PK_MARBASCLASS = A3.PK_MARBASCLASS left join bd_measdoc A4 on A2.PK_MEASDOC = A4.pk_measdoc where A.PK_ORG = '0001A110000000001V70' AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillflag = 4 and substr(A2.code,0,4) != '0915' and A.cwarehouseid not in('1001A1100000000T5S5Z','1001A11000000003CYSY')  and A.cotherwhid  not in('1001A1100000000T5S5Z','1001A11000000003CYSY')";
                }
                else
                {
                    if (updateCount > 0)
                    {
                        sql = "SELECT A.cspecialhid ID,A1.cspecialbid autoid,A1.crowno doclineno,A2.code cinvcode, A2.name cinvname, A3.code cinvclass,A3.name cinvclassname, A2.materialspec cinvstd,A4.code cinvUnit,NVL(A1.nassistnum ,0) qty, A1.vnotebody remark FROM ic_whstrans_h A left join ic_whstrans_b A1 on A1.cspecialhid = A.cspecialhid and A1.dr != 1 left join bd_material A2 on A1.cmaterialvid = A2.pk_material left join bd_marbasclass A3 ON A2.PK_MARBASCLASS = A3.PK_MARBASCLASS left join bd_measdoc A4 on A2.PK_MEASDOC = A4.pk_measdoc where A.PK_ORG = '0001A110000000001V70' AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillflag = 4 and substr(A2.code,0,4) != '0915' and A.cwarehouseid not in('1001A1100000000T5S5Z','1001A11000000003CYSY')  and A.cotherwhid  not in('1001A1100000000T5S5Z','1001A11000000003CYSY') and " + strGetOracleSQLIn + "";
                    }
                    else
                    {
                        sql = "SELECT A.cspecialhid ID,A1.cspecialbid autoid,A1.crowno doclineno,A2.code cinvcode, A2.name cinvname, A3.code cinvclass,A3.name cinvclassname, A2.materialspec cinvstd,A4.code cinvUnit,NVL(A1.nassistnum ,0) qty, A1.vnotebody remark FROM ic_whstrans_h A left join ic_whstrans_b A1 on A1.cspecialhid = A.cspecialhid and A1.dr != 1 left join bd_material A2 on A1.cmaterialvid = A2.pk_material left join bd_marbasclass A3 ON A2.PK_MARBASCLASS = A3.PK_MARBASCLASS left join bd_measdoc A4 on A2.PK_MEASDOC = A4.pk_measdoc where A.PK_ORG = '0001A110000000001V70' AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillflag = 4 and substr(A2.code,0,4) != '0915' and A.cwarehouseid not in('1001A1100000000T5S5Z','1001A11000000003CYSY')  and A.cotherwhid  not in('1001A1100000000T5S5Z','1001A11000000003CYSY')";
                    }
                }
                DataSet IcWhstransHLine = OracleHelper.ExecuteDataset(sql);

                //判断当前表是否存在 1存在 0 不存在
                tableExist = "if object_id( 'TransVouch') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    createSql = "create table TransVouch(ID nvarchar(30) primary key not null,code nvarchar(50),ddate nvarchar(20),cowhcode nvarchar(100),ciwhcode nvarchar(100),codepcode nvarchar(50),cidepcode nvarchar(50),remark nvarchar(100),isRed bit default 0,ts nvarchar(50),zt bit default 0,memo text)";
                    SqlHelperForBulk.ExecuteNonQuery(createSql);
                    //StringBuilder str = DataSetToArrayList.DataSetToArrayLists(IcWhstransH, "TransVouch");
                    //SqlHelperForBulk.ExecuteNonQuery(str.ToString());

                    SqlBulkCopyHelperForTest.ImportTempTableDataIndex(IcWhstransH, "TransVouch");

                    msg = "转库单表插入成功";
                }
                else
                {
                    //StringBuilder str = DataSetToArrayList.DataSetToArrayLists(IcWhstransH, "TransVouch");
                    if (JudgeDs(IcWhstransH))
                    {
                        //SqlHelperForBulk.ExecuteNonQuery(str.ToString());

                        SqlBulkCopyHelperForTest.ImportTempTableDataIndex(IcWhstransH, "TransVouch");
                        msg = "转库单表更新成功";
                    }
                    else
                    {
                        msg = "转库单表暂无可更新数据";
                    }
                }
                tableExist = "if object_id( 'TransVouchs') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    createSql = "create table TransVouchs(ID nvarchar(30),autoid nvarchar(30)  primary key not null,doclineno nvarchar(50),cinvcode nvarchar(50),cinvname nvarchar(500),cinvclass nvarchar(500),cinvclassname nvarchar(500),cinvstd nvarchar(500),cinvUnit  nvarchar(500),qty decimal(28,8),remark nvarchar(100))";
                    SqlHelperForBulk.ExecuteNonQuery(createSql);
                    //StringBuilder strs = DataSetToArrayList.DataSetToArrayLists(IcWhstransHLine, "TransVouchs");
                    //SqlHelperForBulk.ExecuteNonQuery(strs.ToString());


                    SqlBulkCopyHelperForTest.ImportTempTableDataIndex(IcWhstransHLine, "TransVouchs");
                    msg = "转库单表行插入成功";
                }
                else
                {
                    //StringBuilder strs = DataSetToArrayList.DataSetToArrayLists(IcWhstransHLine, "TransVouchs");
                    if (JudgeDs(IcWhstransHLine))
                    {
                        //SqlHelperForBulk.ExecuteNonQuery(strs.ToString());

                        SqlBulkCopyHelperForTest.ImportTempTableDataIndex(IcWhstransHLine, "TransVouchs");
                        msg = "转库单表行更新成功";
                    }
                    else
                    {
                        msg = "转库单表暂无可更新数据";
                    }

                }
                //GetU8SVApiUrlApi("dbdapi");
                result = msg;
            }
            catch (Exception e)
            {

                result = "转库单表错误：" + e.Message;
            }
            return result;

        }



        /// <summary>
        /// 从nc获取销售出库数据插入到sql
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        private string GetSaleOutToSql()
        {
            string result = "";
            string createSql = "";
            string tableExist = "";
            int existResult = 0;
            string msg = "";
            string sql = "";
            int updateCount = 0;
            StringBuilder strbu = new StringBuilder();
            string strGetOracleSQLIn = "";
            try
            {
                //判断当前表是否存在 1存在 0 不存在
                tableExist = "if object_id( 'DispatchList') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    //获取销售出库头数据
                    sql = "select A.cgeneralhid ID,A.vbillcode code,A.dbilldate ddate,A1.Code cwhcode,A5.pk_billtypecode cstcode, A2.code custcode,A2.name custname,A3.code cdepcode,A5.code cCCCode, A.vnote remark,CASE WHEN A.ntotalnum > 0 THEN 0 ELSE 1 END AS isRed,A.modifiedtime ts  from ic_saleout_h A left join bd_stordoc A1 on A.cwarehouseid = A1.Pk_Stordoc left join bd_customer A2 on A.ccustomerid = A2.pk_customer left join org_dept A3 on A3.PK_DEPT = A.cdptid left join bd_customer A4 on A4.pk_customer=A.ccustomerid left join bd_custclass A5 on A5.pk_custclass=A4.pk_custclass left join (select cgeneralhid,csourcetranstype from ic_saleout_b group by cgeneralhid,csourcetranstype) A4 on A.cgeneralhid=A4.cgeneralhid left join bd_billtype A5 on A5.pk_billtypeid=A4.csourcetranstype where  not exists (select cgeneralhid from (select distinct pob.cgeneralhid cgeneralhid from ic_saleout_b pob left join bd_material mat on mat.pk_material = pob.cmaterialvid and nvl(mat.dr,0)=0 left join ic_saleout_h poh on poh.cgeneralhid = pob.cgeneralhid and nvl(poh.dr,0)=0 where  nvl(pob.dr,0)=0 and substr(mat.code,0,4) = '0915' and poh.PK_ORG='0001A110000000001V70'  AND substr(poh.taudittime,0,10) between '" + startTime + "' and '" + endTime + "' and poh.fbillflag=3) po  where po.cgeneralhid = A.cgeneralhid) and  A.PK_ORG='0001A110000000001V70'  AND substr(A.taudittime,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillflag=3 and A.cwarehouseid not in('1001A1100000000T5S5Z','1001A11000000003CYSY')";
                }
                else
                {
                    string delstr = "delete from DispatchList where id in(select ID from DispatchList where zt != 1 )";
                    string delstr2 = "delete from DispatchLists where id in(select ID from DispatchList where zt != 1 )";
                    SqlHelperForBulk.ExecuteNonQuerys(delstr2);
                    SqlHelperForBulk.ExecuteNonQuerys(delstr);
                    string str = "select id from DispatchList";
                    DataSet ds = SqlHelperForBulk.ExecuteDataset(connectionString, CommandType.Text, str);

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        updateCount = ds.Tables[0].Rows.Count;
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            strbu.Append(dr["id"].ToString() + ",");
                        }
                        strbu = strbu.Remove(strbu.Length - 1, 1);
                        String[] ids = strbu.ToString().Split(',');
                        strGetOracleSQLIn = getOracleSQLIn(ids, "A.cgeneralhid");
                        //获取采购入库头数据
                        sql = "select A.cgeneralhid ID,A.vbillcode code,A.dbilldate ddate,A1.Code cwhcode,A5.pk_billtypecode cstcode, A2.code custcode,A2.name custname,A3.code cdepcode,A5.code cCCCode, A.vnote remark,CASE WHEN A.ntotalnum > 0 THEN 0 ELSE 1 END AS isRed,A.modifiedtime ts  from ic_saleout_h A left join bd_stordoc A1 on A.cwarehouseid = A1.Pk_Stordoc left join bd_customer A2 on A.ccustomerid = A2.pk_customer left join org_dept A3 on A3.PK_DEPT = A.cdptid left join bd_customer A4 on A4.pk_customer=A.ccustomerid left join bd_custclass A5 on A5.pk_custclass=A4.pk_custclass left join (select cgeneralhid,csourcetranstype from ic_saleout_b group by cgeneralhid,csourcetranstype) A4 on A.cgeneralhid=A4.cgeneralhid left join bd_billtype A5 on A5.pk_billtypeid=A4.csourcetranstype where  not exists (select cgeneralhid from (select distinct pob.cgeneralhid cgeneralhid from ic_saleout_b pob left join bd_material mat on mat.pk_material = pob.cmaterialvid and nvl(mat.dr,0)=0 left join ic_saleout_h poh on poh.cgeneralhid = pob.cgeneralhid and nvl(poh.dr,0)=0 where  nvl(pob.dr,0)=0 and substr(mat.code,0,4) = '0915' and poh.PK_ORG='0001A110000000001V70'  AND substr(poh.taudittime,0,10) between '" + startTime + "' and '" + endTime + "' and poh.fbillflag=3) po  where po.cgeneralhid = A.cgeneralhid) and  A.PK_ORG='0001A110000000001V70'  AND substr(A.taudittime,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillflag=3 and A.cwarehouseid not in('1001A1100000000T5S5Z','1001A11000000003CYSY') and " + strGetOracleSQLIn + "";

                    }
                    else
                    {
                        updateCount = 0;
                        sql = "select A.cgeneralhid ID,A.vbillcode code,A.dbilldate ddate,A1.Code cwhcode,A5.pk_billtypecode cstcode, A2.code custcode,A2.name custname,A3.code cdepcode,A5.code cCCCode, A.vnote remark,CASE WHEN A.ntotalnum > 0 THEN 0 ELSE 1 END AS isRed,A.modifiedtime ts  from ic_saleout_h A left join bd_stordoc A1 on A.cwarehouseid = A1.Pk_Stordoc left join bd_customer A2 on A.ccustomerid = A2.pk_customer left join org_dept A3 on A3.PK_DEPT = A.cdptid left join bd_customer A4 on A4.pk_customer=A.ccustomerid left join bd_custclass A5 on A5.pk_custclass=A4.pk_custclass left join (select cgeneralhid,csourcetranstype from ic_saleout_b group by cgeneralhid,csourcetranstype) A4 on A.cgeneralhid=A4.cgeneralhid left join bd_billtype A5 on A5.pk_billtypeid=A4.csourcetranstype where  not exists (select cgeneralhid from (select distinct pob.cgeneralhid cgeneralhid from ic_saleout_b pob left join bd_material mat on mat.pk_material = pob.cmaterialvid and nvl(mat.dr,0)=0 left join ic_saleout_h poh on poh.cgeneralhid = pob.cgeneralhid and nvl(poh.dr,0)=0 where  nvl(pob.dr,0)=0 and substr(mat.code,0,4) = '0915' and poh.PK_ORG='0001A110000000001V70'  AND substr(poh.taudittime,0,10) between '" + startTime + "' and '" + endTime + "' and poh.fbillflag=3) po  where po.cgeneralhid = A.cgeneralhid) and  A.PK_ORG='0001A110000000001V70'  AND substr(A.taudittime,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillflag=3 and A.cwarehouseid not in('1001A1100000000T5S5Z','1001A11000000003CYSY')";
                    }
                }
                DataSet SaleOut = OracleHelper.ExecuteDataset(sql);

                tableExist = "if object_id( 'DispatchLists') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    //获取销售出库行数据
                    sql = "select A1.cgeneralhid ID,A1.cgeneralbid autoid,A1.crowno doclineno,A2.code cinvcode,A2.name cinvname,A3.code cinvclass,A3.Name cinvclassname,A2.materialspec cinvstd,A4.code cinvUnit,NVL(A1.nassistnum ,0) qty, A1.ntaxrate itaxrate, A1.norigtaxprice iOriTaxCost,A1.nqtorigprice iOriCost,A1.norigtaxmny ioriSum, A1.norigmny iOriMoney, A1.ntaxmny isum, A1.nmny iMoney, NVL(A1.nqtprice,0) iUnitCost, A1.vnotebody remark from ic_saleout_h A left join ic_saleout_b A1 on A.cgeneralhid = A1.cgeneralhid and A1.DR != 1 left join bd_material A2 on A1.cmaterialvid = A2.pk_material left join bd_marbasclass A3 ON A2.PK_MARBASCLASS = A3.PK_MARBASCLASS left join bd_measdoc A4 on A2.PK_MEASDOC = A4.pk_measdoc  where A.PK_ORG = '0001A110000000001V70' and A.DR != 1 AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillflag = 3 and substr(A2.code,0,4) != '0915' and A.cwarehouseid not in('1001A1100000000T5S5Z','1001A11000000003CYSY')";
                }
                else
                {
                    if (updateCount > 0)
                    {
                        sql = "select A1.cgeneralhid ID,A1.cgeneralbid autoid,A1.crowno doclineno,A2.code cinvcode,A2.name cinvname,A3.code cinvclass,A3.Name cinvclassname,A2.materialspec cinvstd,A4.code cinvUnit,NVL(A1.nassistnum ,0) qty, A1.ntaxrate itaxrate, A1.norigtaxprice iOriTaxCost,A1.nqtorigprice iOriCost,A1.norigtaxmny ioriSum, A1.norigmny iOriMoney, A1.ntaxmny isum, A1.nmny iMoney, NVL(A1.nqtprice,0) iUnitCost, A1.vnotebody remark from ic_saleout_h A left join ic_saleout_b A1 on A.cgeneralhid = A1.cgeneralhid and A1.DR != 1 left join bd_material A2 on A1.cmaterialvid = A2.pk_material left join bd_marbasclass A3 ON A2.PK_MARBASCLASS = A3.PK_MARBASCLASS left join bd_measdoc A4 on A2.PK_MEASDOC = A4.pk_measdoc  where A.PK_ORG = '0001A110000000001V70' and A.DR != 1 AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillflag = 3 and substr(A2.code,0,4) != '0915' and A.cwarehouseid not in('1001A1100000000T5S5Z','1001A11000000003CYSY') and " + strGetOracleSQLIn + "";
                    }
                    else
                    {
                        sql = "select A1.cgeneralhid ID,A1.cgeneralbid autoid,A1.crowno doclineno,A2.code cinvcode,A2.name cinvname,A3.code cinvclass,A3.Name cinvclassname,A2.materialspec cinvstd,A4.code cinvUnit,NVL(A1.nassistnum ,0) qty, A1.ntaxrate itaxrate, A1.norigtaxprice iOriTaxCost,A1.nqtorigprice iOriCost,A1.norigtaxmny ioriSum, A1.norigmny iOriMoney, A1.ntaxmny isum, A1.nmny iMoney, NVL(A1.nqtprice,0) iUnitCost, A1.vnotebody remark from ic_saleout_h A left join ic_saleout_b A1 on A.cgeneralhid = A1.cgeneralhid and A1.DR != 1 left join bd_material A2 on A1.cmaterialvid = A2.pk_material left join bd_marbasclass A3 ON A2.PK_MARBASCLASS = A3.PK_MARBASCLASS left join bd_measdoc A4 on A2.PK_MEASDOC = A4.pk_measdoc  where A.PK_ORG = '0001A110000000001V70' and A.DR != 1 AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillflag = 3 and substr(A2.code,0,4) != '0915' and A.cwarehouseid not in('1001A1100000000T5S5Z','1001A11000000003CYSY')";
                    }
                }
                DataSet SaleOutLine = OracleHelper.ExecuteDataset(sql);

                //判断当前表是否存在 1存在 0 不存在
                tableExist = "if object_id( 'DispatchList') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    createSql = "create table DispatchList(ID nvarchar(30) primary key not null,code nvarchar(50),ddate nvarchar(20),cwhcode nvarchar(100),cstcode nvarchar(200),custcode nvarchar(200),custname nvarchar(200),cdepcode nvarchar(200),cCCCode nvarchar(200),remark nvarchar(500),isRed bit default 0,ts nvarchar(50),zt bit default 0,memo text)";
                    SqlHelperForBulk.ExecuteNonQuery(createSql);

                    //StringBuilder str = DataSetToArrayList.DataSetToArrayLists(SaleOut, "DispatchList");
                    //SqlHelperForBulk.ExecuteNonQuery(str.ToString());

                    SqlBulkCopyHelperForTest.ImportTempTableDataIndex(SaleOut, "DispatchList");
                    msg = "销售出库表插入成功";
                }
                else
                {
                    //StringBuilder str = DataSetToArrayList.DataSetToArrayLists(SaleOut, "DispatchList");
                    if (JudgeDs(SaleOut))
                    {
                        //SqlHelperForBulk.ExecuteNonQuery(str.ToString());

                        SqlBulkCopyHelperForTest.ImportTempTableDataIndex(SaleOut, "DispatchList");
                        msg = "销售出库表更新成功";
                    }
                    else
                    {
                        msg = "销售出库表暂无可更新数据";
                    }
                }
                tableExist = "if object_id( 'DispatchLists') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    createSql = "create table DispatchLists(ID nvarchar(30),autoid nvarchar(30)  primary key not null,doclineno nvarchar(50),cinvcode nvarchar(50),cinvname nvarchar(500),cinvclass nvarchar(500),cinvclassname nvarchar(500),cinvstd nvarchar(500),cinvUnit  nvarchar(500),qty decimal(28, 8),itaxrate decimal(28, 8),iOriTaxCost decimal(28, 8),iOriCost decimal(28, 8),ioriSum decimal(28, 8),	iOriMoney decimal(28, 8),	isum decimal(28, 8),iMoney decimal(28, 8),iUnitCost decimal(28, 8),remark nvarchar(100))";
                    SqlHelperForBulk.ExecuteNonQuery(createSql);
                    //StringBuilder strs = DataSetToArrayList.DataSetToArrayLists(SaleOutLine, "DispatchLists");
                    //SqlHelperForBulk.ExecuteNonQuery(strs.ToString());

                    SqlBulkCopyHelperForTest.ImportTempTableDataIndex(SaleOutLine, "DispatchLists");
                    msg = "销售出库表行插入成功";
                }
                else
                {
                    //StringBuilder strs = DataSetToArrayList.DataSetToArrayLists(SaleOutLine, "DispatchLists");
                    if (JudgeDs(SaleOutLine))
                    {
                        //SqlHelperForBulk.ExecuteNonQuery(strs.ToString());

                        SqlBulkCopyHelperForTest.ImportTempTableDataIndex(SaleOutLine, "DispatchLists");
                        msg = "销售出库表行更新成功";
                    }
                    else
                    {
                        msg = "销售出库表暂无可更新数据";
                    }
                }
                //GetU8SVApiUrlApi("fhdapi");
                result = msg;
            }
            catch (Exception e)
            {

                result = "销售出库表：" + e.Message;
            }
            return result;

        }


        /// <summary>
        /// 从nc获取采购发票数据插入到sql
        /// 创建人：lvhe
        /// 创建时间：2019年10月13日 22:59:30
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        private string GetPurchaseInvoicesToSql()
        {
            string result = "";
            string createSql = "";
            string tableExist = "";
            int existResult = 0;
            string msg = "";
            string sql = "";
            StringBuilder strbu = new StringBuilder();
            string strGetOracleSQLIn = "";
            DataSet sqlServerInvoices = new DataSet();
            int updateCount = 0;
            try
            {
                //判断当前表是否存在 1存在 0 不存在
                tableExist = "if object_id( 'PurBillVouch') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    //获取采购发票头数据
                    sql = " select A.PK_INVOICE ID,A.vbillcode code,A.DBILLDATE ddate,A.darrivedate darrivedate,A1.code cvencode,A2.CODE cdepcode,CASE WHEN A.ntotalastnum>0 THEN 0 ELSE 1 END AS isRed,A.VMEMO remark,A.modifiedtime ts from po_invoice A left join org_dept A2 on A2.PK_DEPT=A.PK_DEPT  and nvl(A2.dr,0)=0 left join bd_supplier A1 on A.PK_SUPPLIER=A1.PK_SUPPLIER and nvl(A1.dr,0)=0 where  not exists (select pk_invoice from (select distinct pob.pk_invoice pk_invoice from po_invoice_b pob left join bd_material mat on mat.pk_material = pob.Pk_material and nvl(mat.dr,0)=0 left join po_invoice poh on poh.PK_INVOICE = pob.PK_INVOICE and nvl(poh.dr,0)=0 where  nvl(pob.dr,0)=0 and substr(mat.code,0,4) = '0915' and poh.PK_ORG='0001A110000000001V70'  AND substr(poh.taudittime,0,10) between '" + startTime + "' and '" + endTime + "' and poh.fbillstatus=3) po  where po.pk_invoice = A.pk_invoice) and  A.PK_ORG='0001A110000000001V70'  AND substr(A.taudittime,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillstatus=3";
                }
                else
                {
                    string delstr = "delete from PurBillVouch where id in(select ID from PurBillVouch where zt != 1 )";
                    string delstr2 = "delete from PurBillVouchs where id in(select ID from PurBillVouch where zt != 1 )";
                    SqlHelperForBulk.ExecuteNonQuerys(delstr2);
                    SqlHelperForBulk.ExecuteNonQuerys(delstr);
                    string str = "select id from PurBillVouch";
                    DataSet ds = SqlHelperForBulk.ExecuteDataset(connectionString, CommandType.Text, str);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        updateCount = ds.Tables[0].Rows.Count;
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            strbu.Append(dr["id"].ToString() + ",");
                        }
                        strbu = strbu.Remove(strbu.Length - 1, 1);
                        String[] ids = strbu.ToString().Split(',');
                        strGetOracleSQLIn = getOracleSQLIn(ids, "A.PK_INVOICE");
                        //获取采购发票头数据
                        sql = " select A.PK_INVOICE ID,A.vbillcode code,A.DBILLDATE ddate,A.darrivedate darrivedate,A1.code cvencode,A2.CODE cdepcode,CASE WHEN A.ntotalastnum>0 THEN 0 ELSE 1 END AS isRed,A.VMEMO remark,A.modifiedtime ts from po_invoice A left join org_dept A2 on A2.PK_DEPT=A.PK_DEPT  and nvl(A2.dr,0)=0 left join bd_supplier A1 on A.PK_SUPPLIER=A1.PK_SUPPLIER and nvl(A1.dr,0)=0 where  not exists (select pk_invoice from (select distinct pob.pk_invoice pk_invoice from po_invoice_b pob left join bd_material mat on mat.pk_material = pob.Pk_material and nvl(mat.dr,0)=0 left join po_invoice poh on poh.PK_INVOICE = pob.PK_INVOICE and nvl(poh.dr,0)=0 where  nvl(pob.dr,0)=0 and substr(mat.code,0,4) = '0915' and poh.PK_ORG='0001A110000000001V70'  AND substr(poh.taudittime,0,10) between '" + startTime + "' and '" + endTime + "' and poh.fbillstatus=3) po  where po.pk_invoice = A.pk_invoice) and  A.PK_ORG='0001A110000000001V70'  AND substr(A.taudittime,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillstatus=3 and " + strGetOracleSQLIn + "";
                    }
                    else
                    {
                        updateCount = 0;
                        sql = " select A.PK_INVOICE ID,A.vbillcode code,A.DBILLDATE ddate,A.darrivedate darrivedate,A1.code cvencode,A2.CODE cdepcode,CASE WHEN A.ntotalastnum>0 THEN 0 ELSE 1 END AS isRed,A.VMEMO remark,A.modifiedtime ts from po_invoice A left join org_dept A2 on A2.PK_DEPT=A.PK_DEPT  and nvl(A2.dr,0)=0 left join bd_supplier A1 on A.PK_SUPPLIER=A1.PK_SUPPLIER and nvl(A1.dr,0)=0 where  not exists (select pk_invoice from (select distinct pob.pk_invoice pk_invoice from po_invoice_b pob left join bd_material mat on mat.pk_material = pob.Pk_material and nvl(mat.dr,0)=0 left join po_invoice poh on poh.PK_INVOICE = pob.PK_INVOICE and nvl(poh.dr,0)=0 where  nvl(pob.dr,0)=0 and substr(mat.code,0,4) = '0915' and poh.PK_ORG='0001A110000000001V70'  AND substr(poh.taudittime,0,10) between '" + startTime + "' and '" + endTime + "' and poh.fbillstatus=3) po  where po.pk_invoice = A.pk_invoice) and  A.PK_ORG='0001A110000000001V70'  AND substr(A.taudittime,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillstatus=3";
                    }
                }
                DataSet Invoices = OracleHelper.ExecuteDataset(sql);

                //获取采购发票行数据
                //判断当前表是否存在 1存在 0 不存在
                tableExist = "if object_id( 'PurBillVouchs') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    sql = "select A.PK_INVOICE ID,A1.pk_invoice_b autoid,A1.vsourcecode srcdocno,A1.crowno doclineno,A1.vsourcerowno srcdoclineno,A2.code cinvcode,NVL(A1.nastnum ,0) qty,A1.ntaxrate itaxrate, A1.nastorigtaxprice iOriTaxCost, A1.nastorigprice iOriCost, A1.norigtaxmny ioriSum,A1.nnosubtax iOriTaxPrice, A1.norigmny iOriMoney, A1.ntaxmny isum, A1.nmny iMoney, A1.ntax iTaxPrice,A1.nastorigtaxprice iUnitCost, A1.vmemob remark from po_invoice A left join po_invoice_b A1 on A.PK_INVOICE = A1.PK_INVOICE and A1.DR!=1  left join bd_material A2 on A1.pk_material = A2.pk_material where A.PK_ORG = '0001A110000000001V70' and A.DR != 1 AND substr(A.taudittime,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillstatus = 3 and substr(A2.code,0,4) != '0915'";
                }
                else
                {
                    if (updateCount > 0)
                    {
                        sql = "select A.PK_INVOICE ID,A1.pk_invoice_b autoid,A1.vsourcecode srcdocno,A1.crowno doclineno,A1.vsourcerowno srcdoclineno,A2.code cinvcode,NVL(A1.nastnum ,0) qty,A1.ntaxrate itaxrate, A1.nastorigtaxprice iOriTaxCost, A1.nastorigprice iOriCost, A1.norigtaxmny ioriSum,A1.nnosubtax iOriTaxPrice, A1.norigmny iOriMoney, A1.ntaxmny isum, A1.nmny iMoney, A1.ntax iTaxPrice,A1.nastorigtaxprice iUnitCost, A1.vmemob remark from po_invoice A left join po_invoice_b A1 on A.PK_INVOICE = A1.PK_INVOICE and A1.DR!=1  left join bd_material A2 on A1.pk_material = A2.pk_material where A.PK_ORG = '0001A110000000001V70' and A.DR != 1 AND substr(A.taudittime,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillstatus = 3 and substr(A2.code,0,4) != '0915' and " + strGetOracleSQLIn + "";
                    }
                    else
                    {
                        sql = "select A.PK_INVOICE ID,A1.pk_invoice_b autoid,A1.vsourcecode srcdocno,A1.crowno doclineno,A1.vsourcerowno srcdoclineno,A2.code cinvcode,NVL(A1.nastnum ,0) qty,A1.ntaxrate itaxrate, A1.nastorigtaxprice iOriTaxCost, A1.nastorigprice iOriCost, A1.norigtaxmny ioriSum,A1.nnosubtax iOriTaxPrice, A1.norigmny iOriMoney, A1.ntaxmny isum, A1.nmny iMoney, A1.ntax iTaxPrice,A1.nastorigtaxprice iUnitCost, A1.vmemob remark from po_invoice A left join po_invoice_b A1 on A.PK_INVOICE = A1.PK_INVOICE and A1.DR!=1  left join bd_material A2 on A1.pk_material = A2.pk_material where A.PK_ORG = '0001A110000000001V70' and A.DR != 1 AND substr(A.taudittime,0,10) between '" + startTime + "' and '" + endTime + "' and A.fbillstatus = 3 and substr(A2.code,0,4) != '0915'";
                    }
                }
                DataSet InvoiceLine = OracleHelper.ExecuteDataset(sql);

                //判断当前表是否存在 1存在 0 不存在
                tableExist = "if object_id( 'PurBillVouch') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    createSql = "create table PurBillVouch(ID nvarchar(30) primary key not null,code nvarchar(50),ddate nvarchar(20),darrivedate nvarchar(20),cvencode nvarchar(50),cdepcode nvarchar(50),isRed bit,remark nvarchar(100),ts nvarchar(50),zt bit default 0,memo text)";
                    SqlHelperForBulk.ExecuteNonQuery(createSql);
                    //StringBuilder str = DataSetToArrayList.DataSetToArrayLists(Invoices, "PurBillVouch");
                    //SqlHelperForBulk.ExecuteNonQuery(str.ToString());

                    SqlBulkCopyHelperForTest.ImportTempTableDataIndex(Invoices, "PurBillVouch");
                    msg = "采购发票表插入成功";
                }
                else
                {
                    //StringBuilder str = DataSetToArrayList.DataSetToArrayLists(Invoices, "PurBillVouch");
                    if (JudgeDs(Invoices))
                    {
                        //SqlHelperForBulk.ExecuteNonQuery(str.ToString());

                        SqlBulkCopyHelperForTest.ImportTempTableDataIndex(Invoices, "PurBillVouch");
                        msg = "采购发票表更新成功";
                    }
                    else
                    {
                        msg = "采购发票表暂无无可更新数据";
                    }
                }
                tableExist = "if object_id( 'PurBillVouchs') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    createSql = "create table PurBillVouchs(ID nvarchar(30),autoid nvarchar(30) primary key not null,srcdocno nvarchar(50),doclineno nvarchar(30),srcdoclineno nvarchar(50),cinvcode nvarchar(50),qty decimal(28, 8),itaxrate decimal(28, 8),iOriTaxCost decimal(28, 8),iOriCost decimal(28, 8),ioriSum decimal(28, 8),iOriTaxPrice decimal(28, 8),iOriMoney decimal(28, 8),isum decimal(28, 8),iMoney decimal(28, 8),iTaxPrice decimal(28, 8),iUnitCost decimal(28, 8),remark nvarchar(100))";
                    SqlHelperForBulk.ExecuteNonQuery(createSql);
                    //StringBuilder strs = DataSetToArrayList.DataSetToArrayLists(InvoiceLine, "PurBillVouchs");
                    //SqlHelperForBulk.ExecuteNonQuery(strs.ToString());

                    SqlBulkCopyHelperForTest.ImportTempTableDataIndex(InvoiceLine, "PurBillVouchs");
                    msg = "采购发票表行插入成功";
                }
                else
                {
                    //StringBuilder strs = DataSetToArrayList.DataSetToArrayLists(InvoiceLine, "PurBillVouchs");
                    if (JudgeDs(InvoiceLine))
                    {
                        //SqlHelperForBulk.ExecuteNonQuery(strs.ToString());

                        SqlBulkCopyHelperForTest.ImportTempTableDataIndex(InvoiceLine, "PurBillVouchs");
                        msg = "采购发票表行更新成功";
                    }
                    else
                    {
                        msg = "采购发票表暂无可更新数据";
                    }
                }
                //GetU8SVApiUrlApi("cgfpapi");
                result = msg;
            }
            catch (Exception e)
            {

                result = "采购发票表行错误：" + e.Message;
            }
            return result;

        }


        /// <summary>
        /// 从nc获取销售发票数据插入到sql
        /// 创建人：lvhe
        /// 创建时间：2019-10-18 13:39:06
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        private string GetSoSaleinvoiceToSql()
        {
            string result = "";
            string createSql = "";
            string tableExist = "";
            int existResult = 0;
            string msg = "";
            string sql = "";
            int updateCount = 0;
            StringBuilder strbu = new StringBuilder();
            string strGetOracleSQLIn = "";
            try
            {
                //判断当前表是否存在 1存在 0 不存在
                tableExist = "if object_id( 'SaleBillVouch') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    //获取销售发票头数据
                    sql = " select A.csaleinvoiceid ID,A.vbillcode code,A.dbilldate ddate,A1.Code custcode,A.vnote remark,CASE WHEN A.ntotalastnum>0 THEN 0 ELSE 1 END AS isRed,A.modifiedtime ts from so_saleinvoice A left join bd_customer A1 on A.cinvoicecustid = A1.pk_customer where  not exists (select csaleinvoiceid from (select distinct pob.csaleinvoiceid csaleinvoiceid from so_saleinvoice_b pob left join bd_material mat on mat.pk_material = pob.cmaterialvid and nvl(mat.dr,0)=0 left join so_saleinvoice poh on poh.csaleinvoiceid = pob.csaleinvoiceid and nvl(poh.dr,0)=0 where  nvl(pob.dr,0)=0  and substr(mat.code,0,4) = '0915' and poh.PK_ORG='0001A110000000001V70'  and poh.dr != 1 AND substr(poh.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and pob.csendstordocid not in('1001A1100000000T5S5Z','1001A11000000003CYSY')) po  where po.csaleinvoiceid = A.csaleinvoiceid) and  A.PK_ORG='0001A110000000001V70' and A.dr != 1  AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "'";
                }
                else
                {
                    string delstr = "delete from SaleBillVouch where id in(select ID from SaleBillVouch where zt != 1 )";
                    string delstr2 = "delete from SaleBillVouchs where id in(select ID from SaleBillVouch where zt != 1 )";
                    SqlHelperForBulk.ExecuteNonQuerys(delstr2);
                    SqlHelperForBulk.ExecuteNonQuerys(delstr);
                    string str = "select id from SaleBillVouch";
                    DataSet ds = SqlHelperForBulk.ExecuteDataset(connectionString, CommandType.Text, str);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        updateCount = ds.Tables[0].Rows.Count;
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            strbu.Append(dr["id"].ToString() + ",");
                        }
                        strbu = strbu.Remove(strbu.Length - 1, 1);
                        String[] ids = strbu.ToString().Split(',');
                        strGetOracleSQLIn = getOracleSQLIn(ids, "A.csaleinvoiceid");
                        //获取采购入库头数据
                        sql = " select A.csaleinvoiceid ID,A.vbillcode code,A.dbilldate ddate,A1.Code custcode,A.vnote remark,CASE WHEN A.ntotalastnum>0 THEN 0 ELSE 1 END AS isRed,A.modifiedtime ts from so_saleinvoice A left join bd_customer A1 on A.cinvoicecustid = A1.pk_customer where  not exists (select csaleinvoiceid from (select distinct pob.csaleinvoiceid csaleinvoiceid from so_saleinvoice_b pob left join bd_material mat on mat.pk_material = pob.cmaterialvid and nvl(mat.dr,0)=0 left join so_saleinvoice poh on poh.csaleinvoiceid = pob.csaleinvoiceid and nvl(poh.dr,0)=0 where  nvl(pob.dr,0)=0  and substr(mat.code,0,4) = '0915' and poh.PK_ORG='0001A110000000001V70'  and poh.dr != 1 AND substr(poh.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and pob.csendstordocid not in('1001A1100000000T5S5Z','1001A11000000003CYSY')) po  where po.csaleinvoiceid = A.csaleinvoiceid) and  A.PK_ORG='0001A110000000001V70' and A.dr != 1  AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and " + strGetOracleSQLIn + "";
                    }
                    else
                    {
                        updateCount = 0;
                        sql = " select A.csaleinvoiceid ID,A.vbillcode code,A.dbilldate ddate,A1.Code custcode,A.vnote remark,CASE WHEN A.ntotalastnum>0 THEN 0 ELSE 1 END AS isRed,A.modifiedtime ts from so_saleinvoice A left join bd_customer A1 on A.cinvoicecustid = A1.pk_customer where  not exists (select csaleinvoiceid from (select distinct pob.csaleinvoiceid csaleinvoiceid from so_saleinvoice_b pob left join bd_material mat on mat.pk_material = pob.cmaterialvid and nvl(mat.dr,0)=0 left join so_saleinvoice poh on poh.csaleinvoiceid = pob.csaleinvoiceid and nvl(poh.dr,0)=0 where  nvl(pob.dr,0)=0  and substr(mat.code,0,4) = '0915' and poh.PK_ORG='0001A110000000001V70'  and poh.dr != 1 AND substr(poh.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and pob.csendstordocid not in('1001A1100000000T5S5Z','1001A11000000003CYSY')) po  where po.csaleinvoiceid = A.csaleinvoiceid) and  A.PK_ORG='0001A110000000001V70' and A.dr != 1  AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "'";
                    }
                }
                DataSet Saleinvoice = OracleHelper.ExecuteDataset(sql);


                tableExist = "if object_id( 'SaleBillVouchs') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    //获取销售发票行数据
                    sql = "select A.csaleinvoiceid ID,A1.csaleinvoicebid autoid,A1.crowno doclineno,A1.vsrccode srcdocno, A1.vsrcrowno srcdoclineno, A2.code cinvcode, NVL(A1.nastnum ,0) qty,A1.ntaxrate itaxrate, A1.nqtorigtaxprice iOriTaxCost, A1.nqtorigprice iOriCost,A1.norigtaxmny ioriSum, A1.ntax iOriTaxPrice, A1.norigmny iOriMoney, A1.ntaxmny isum,A1.nmny iMoney,(A1.ntaxmny - A1.nmny) iTaxPrice,A1.nqttaxprice iUnitCost, A1.vrownote remark,A1.csendstordocid cwhcode from so_saleinvoice A left join so_saleinvoice_b A1 on A.csaleinvoiceid = A1.csaleinvoiceid and A1.dr!=1 left join bd_material A2 on A1.cmaterialvid = A2.pk_material where A.PK_ORG = '0001A110000000001V70' and A.dr != 1 AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and substr(A2.code,0,4) != '0915' and A1.csendstordocid not in('1001A1100000000T5S5Z','1001A11000000003CYSY')";
                }
                else
                {
                    if (updateCount > 0)
                    {
                        //获取销售发票行数据
                        sql = "select A.csaleinvoiceid ID,A1.csaleinvoicebid autoid,A1.crowno doclineno,A1.vsrccode srcdocno, A1.vsrcrowno srcdoclineno, A2.code cinvcode, NVL(A1.nastnum ,0) qty,A1.ntaxrate itaxrate, A1.nqtorigtaxprice iOriTaxCost, A1.nqtorigprice iOriCost,A1.norigtaxmny ioriSum, A1.ntax iOriTaxPrice, A1.norigmny iOriMoney, A1.ntaxmny isum,A1.nmny iMoney,(A1.ntaxmny - A1.nmny) iTaxPrice,A1.nqttaxprice iUnitCost, A1.vrownote remark,A1.csendstordocid cwhcode from so_saleinvoice A left join so_saleinvoice_b A1 on A.csaleinvoiceid = A1.csaleinvoiceid and A1.dr!=1 left join bd_material A2 on A1.cmaterialvid = A2.pk_material where A.PK_ORG = '0001A110000000001V70' and A.dr != 1 AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and substr(A2.code,0,4) != '0915' and A1.csendstordocid not in('1001A1100000000T5S5Z','1001A11000000003CYSY') and  " + strGetOracleSQLIn + "";
                    }
                    else
                    {
                        sql = "select A.csaleinvoiceid ID,A1.csaleinvoicebid autoid,A1.crowno doclineno,A1.vsrccode srcdocno, A1.vsrcrowno srcdoclineno, A2.code cinvcode, NVL(A1.nastnum ,0) qty,A1.ntaxrate itaxrate, A1.nqtorigtaxprice iOriTaxCost, A1.nqtorigprice iOriCost,A1.norigtaxmny ioriSum, A1.ntax iOriTaxPrice, A1.norigmny iOriMoney, A1.ntaxmny isum,A1.nmny iMoney,(A1.ntaxmny - A1.nmny) iTaxPrice,A1.nqttaxprice iUnitCost, A1.vrownote remark,A1.csendstordocid cwhcode from so_saleinvoice A left join so_saleinvoice_b A1 on A.csaleinvoiceid = A1.csaleinvoiceid and A1.dr!=1 left join bd_material A2 on A1.cmaterialvid = A2.pk_material where A.PK_ORG = '0001A110000000001V70' and A.dr != 1 AND substr(A.dbilldate,0,10) between '" + startTime + "' and '" + endTime + "' and substr(A2.code,0,4) != '0915' and A1.csendstordocid not in('1001A1100000000T5S5Z','1001A11000000003CYSY')";
                    }
                }
                DataSet SaleinvoiceLine = OracleHelper.ExecuteDataset(sql);

                //判断当前表是否存在 1存在 0 不存在
                tableExist = "if object_id( 'SaleBillVouch') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    createSql = "create table SaleBillVouch(ID nvarchar(30) primary key not null,code nvarchar(50),ddate nvarchar(20),custcode nvarchar(50),remark nvarchar(100),isRed bit,ts nvarchar(50),zt bit default 0,memo text)";
                    SqlHelperForBulk.ExecuteNonQuery(createSql);
                    //StringBuilder str = DataSetToArrayList.DataSetToArrayLists(Saleinvoice, "SaleBillVouch");
                    //SqlHelperForBulk.ExecuteNonQuery(str.ToString());

                    SqlBulkCopyHelperForTest.ImportTempTableDataIndex(Saleinvoice, "SaleBillVouch");
                    msg = "销售发票表插入成功";
                }
                else
                {
                    //StringBuilder str = DataSetToArrayList.DataSetToArrayLists(Saleinvoice, "SaleBillVouch");
                    if (JudgeDs(Saleinvoice))
                    {
                        //SqlHelperForBulk.ExecuteNonQuery(str.ToString());
                        SqlBulkCopyHelperForTest.ImportTempTableDataIndex(Saleinvoice, "SaleBillVouch");
                        msg = "销售发票表更新成功";
                    }
                    else
                    {
                        msg = "销售发票表暂无可更新数据";
                    }
                }
                tableExist = "if object_id( 'SaleBillVouchs') is not null select 1 else select 0";
                existResult = SqlHelperForBulk.ExecuteNonQuerys(tableExist);

                if (existResult == 0)
                {
                    createSql = "create table SaleBillVouchs(ID nvarchar(30),autoid nvarchar(30)  primary key not null,doclineno nvarchar(50),srcdocno nvarchar(50),	srcdoclineno nvarchar(50),cinvcode nvarchar(50),qty decimal(28,8),itaxrate decimal(28,8),iOriTaxCost decimal(28,8),iOriCost decimal(28,8),ioriSum decimal(28,8),iOriTaxPrice decimal(28,8),iOriMoney decimal(28,8),isum decimal(28,8),iMoney decimal(28,8),	iTaxPrice decimal(28,8),	iUnitCost decimal(28,8),remark nvarchar(100),cwhcode nvarchar(100))";
                    SqlHelperForBulk.ExecuteNonQuery(createSql);
                    //StringBuilder strs = DataSetToArrayList.DataSetToArrayLists(SaleinvoiceLine, "SaleBillVouchs");
                    //SqlHelperForBulk.ExecuteNonQuery(strs.ToString());

                    SqlBulkCopyHelperForTest.ImportTempTableDataIndex(SaleinvoiceLine, "SaleBillVouchs");
                    msg = "销售发票表行插入成功";
                }
                else
                {
                    //StringBuilder strs = DataSetToArrayList.DataSetToArrayLists(SaleinvoiceLine, "SaleBillVouchs");
                    if (JudgeDs(SaleinvoiceLine))
                    {
                        //SqlHelperForBulk.ExecuteNonQuery(strs.ToString());

                        SqlBulkCopyHelperForTest.ImportTempTableDataIndex(SaleinvoiceLine, "SaleBillVouchs");
                        msg = "销售发票表行更新成功";
                    }
                    else
                    {
                        msg = "销售发票表暂无可更新数据";
                    }
                }
                //GetU8SVApiUrlApi("xsfpapi");
                result = msg;
            }
            catch (Exception e)
            {

                result = "销售发票表错误：" + e.Message;
            }
            return result;

        }



        /// <summary>
        /// 处理 Oracle SQL in 超过1000 的解决方案
        /// 创建人：lvhe
        /// 创建时间：2019年12月31日 01:20:36
        /// </summary>
        /// <param name="sqlParam"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        private string getOracleSQLIn(string[] ids, string field)
        {
            int count = Math.Min(ids.Length, 1000);
            int len = ids.Length;
            int size = len % count;
            if (size == 0)
            {
                size = len / count;
            }
            else
            {
                size = (len / count) + 1;
            }
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < size; i++)
            {
                int fromIndex = i * count;
                int toIndex = Math.Min(fromIndex + count, len);
                string productId = string.Join("','", getArrayValues(fromIndex, toIndex, ids).ToArray());
                if (i != 0)
                {
                    builder.Append(" and ");
                }
                builder.Append(field).Append(" not in ('").Append(productId).Append("')");
            }
            return builder.ToString();
        }


        /// <summary>
        /// 处理 Oracle SQL in 超过1000 的解决方案
        /// 创建人：lvhe
        /// 创建时间：2019年12月31日 01:20:36
        /// </summary>
        /// <param name="sqlParam"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public List<string> getArrayValues(int fromindex, int toindex, string[] array)
        {
            List<string> listret = new List<string>();
            for (int i = fromindex; i < toindex; i++)
            {
                listret.Add(array[i]);
            }
            return listret;
        }


        /// <summary>  
        /// 判断DS是否为空  
        /// </summary>  
        /// <param name="ds">需要判断的ds</param>  
        /// <returns>如果ds为空，返回true</returns>  
        private bool JudgeDs(DataSet ds)
        {
            bool Flag = true;
            if ((ds == null) || (ds.Tables.Count == 0) || (ds.Tables.Count == 1 && ds.Tables[0].Rows.Count == 0))
            {
                Flag = false;
            }
            return Flag;
        }



        /// <summary>
        /// 调用凭证Api接口
        /// 创建人:lvhe
        /// 创建时间：2020-3-10 14:13:51
        /// </summary>
        /// <returns></returns>
        private string GetU8SVApiUrlApi(string apiParams)
        {
            string interfaceParameters = apiParams;

            string U8SVApiUrl = u8SVApiUrl;
            string webApiUrl = ""; //调用url
            string webApiType = "Get";//请求方式
            string webApiParam = "";//调用参数
            switch (interfaceParameters)
            {
                //查询
                //case "chaxu":
                //    webApiUrl = "http://erp.test.cvming.com/u8sv/ERPU8/Rdrecord01Add";
                //    break;
                //采购入库单接口
                case "cgrkapi":
                    webApiUrl = U8SVApiUrl + "ERPU8/Rdrecord01Add";
                    webApiType = "Post";
                    webApiParam = "cgrkapi";
                    break;
                //采购发票接口
                case "cgfpapi":
                    webApiUrl = U8SVApiUrl + "ERPU8/PurBillVouchAdd";
                    webApiType = "Post";
                    webApiParam = "cgfpapi";
                    break;
                //销售发票接口
                case "xsfpapi":
                    webApiUrl = U8SVApiUrl + "ERPU8/SaleBillVouchAdd";
                    webApiType = "Post";
                    webApiParam = "xsfpapi";
                    break;
                //发货单接口
                case "fhdapi":
                    webApiUrl = U8SVApiUrl + "ERPU8/DispatchListAdd";
                    webApiType = "Post";
                    webApiParam = "fhdapi";
                    break;
                //材料出库接口
                case "clckapi":
                    webApiUrl = U8SVApiUrl + "ERPU8/RdRecord11Add";
                    webApiType = "Post";
                    webApiParam = "clckapi";
                    break;
                //产成品入库接口
                case "ccprkapi":
                    webApiUrl = U8SVApiUrl + "ERPU8/RdRecord10Add";
                    webApiType = "Post";
                    webApiParam = "ccprkapi";
                    break;
                //其他入库单接口
                case "qtrkdapi":
                    webApiUrl = U8SVApiUrl + "ERPU8/RdRecord08Add";
                    webApiType = "Post";
                    webApiParam = "qtrkdapi";
                    break;
                //其他出库单接口
                case "qtckdapi":
                    webApiUrl = U8SVApiUrl + "ERPU8/RdRecord09Add";
                    webApiType = "Post";
                    webApiParam = "qtckdapi";
                    break;
                //调拨单接口
                case "dbdapi":
                    webApiUrl = U8SVApiUrl + "ERPU8/TransVouchAdd";
                    webApiType = "Post";
                    webApiParam = "dbdapi";
                    break;
                //形态转换单单接口
                case "xtzhdapi":
                    webApiUrl = U8SVApiUrl + "ERPU8/AssemVouchAdd";
                    webApiType = "Post";
                    webApiParam = "xtzhdapi";
                    break;
                //凭证单接口
                case "pingzApi":
                    webApiUrl = U8SVApiUrl + "ERPU8/CreateGL";
                    webApiType = "Post";
                    webApiParam = "pingzApi";
                    break;
                //物料接口
                case "wulApi":
                    webApiUrl = U8SVApiUrl + "ERPU8/InventoryUpdate";
                    webApiType = "Post";
                    webApiParam = "wulApi";
                    break;
                //供应商接口
                case "gysApi":
                    webApiUrl = U8SVApiUrl + "ERPU8/SupplierUpdate";
                    webApiType = "Post";
                    webApiParam = "gysApi";
                    break;
                //客户接口
                case "kehuApi":
                    webApiUrl = U8SVApiUrl + "ERPU8/CustomerUpdate";
                    webApiType = "Post";
                    webApiParam = "kehuApi";
                    break;
                //部门接口
                case "deptApi":
                    webApiUrl = U8SVApiUrl + "ERPU8/DepartmentAdd";
                    webApiType = "Post";
                    webApiParam = "deptApi";
                    break;
                //批量更新产成品入库api接口
                case "plgxccprkapi":
                    webApiUrl = U8SVApiUrl + "ERPU8/RdRecord10ItemUpdate";
                    webApiType = "Post";
                    webApiParam = "plgxccprkapi";
                    break;
                case "djshapi":
                    webApiUrl = U8SVApiUrl + "ERPU8/DoApprove";
                    webApiType = "Post";
                    webApiParam = "djshapi";
                    break;
                case "wlqdApi":
                    webApiUrl = U8SVApiUrl + "ERPU8/CreateBOM";
                    webApiType = "Post";
                    webApiParam = "wlqdApi";
                    break;
                case "ckxxapi":
                    webApiUrl = U8SVApiUrl + "ERPU8/WarehouseAdd";
                    webApiType = "Post";
                    webApiParam = "ckxxapi";
                    break;
                //ckxxapi
                default:
                    webApiUrl = "请检查参数是否正确,没有找到输入参数对应的接口信息！";
                    break;
            }
            //接口返回信息
            string msg = "";
            if (!string.IsNullOrEmpty(webApiUrl) && webApiUrl != "请检查参数是否正确,没有找到输入参数对应的接口信息！")
            {
                try
                {
                    msg = HttpApi(webApiUrl, "{}", webApiType, webApiParam);
                }
                catch (Exception e)
                {
                    msg = e.Message;
                }
            }
            else
            {
                msg = "请检查参数是否正确,没有找到输入参数对应的接口信息！";
            }
            return msg;
        }



        #region <<调用webApi接口>>
        /// <summary>
        /// 调用api返回json
        /// </summary>
        /// <param name="url">api地址</param>
        /// <param name="jsonstr">接收参数</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static string HttpApi(string url, string jsonstr, string type, string webApiParam)
        {
            string result = "";
            if (type.ToUpper().ToString() == "POST")
            {
                result = HttpPost(url, jsonstr, type, webApiParam);
            }
            else
            {
                result = HttpGet(url, webApiParam);
            }
            return result;
        }

        public static string HttpGet(string url, string webApiParam)
        {
            string msg = "";
            switch (webApiParam)
            {
                //查询
                //case "chaxu":
                //    webApiUrl = "http://erp.test.cvming.com/u8sv/ERPU8/Rdrecord01Add";
                //    break;
                //采购入库单接口
                case "cgrkapi":
                    msg = "采购入库单接口";
                    break;
                //采购发票接口
                case "cgfpapi":
                    msg = "采购发票接口";
                    break;
                //销售发票接口
                case "xsfpapi":
                    msg = "销售发票接口";
                    break;
                //发货单接口
                case "fhdapi":
                    msg = "发货单接口";
                    break;
                //材料出库接口
                case "clckapi":
                    msg = "材料出库接口";
                    break;
                //产成品入库接口
                case "ccprkapi":
                    msg = "产成品入库接口";
                    break;
                //其他入库单接口
                case "qtrkdapi":
                    msg = "销售发票接口";
                    break;
                //部门接口
                case "deptApi":
                    msg = "部门档案接口";
                    break;
                //其他出库单接口
                case "qtckdapi":
                    msg = "其他出库单接口";
                    break;
                //调拨单接口
                case "dbdapi":
                    msg = "调拨单接口";
                    break;
                //形态转换单单接口
                case "xtzhdapi":
                    msg = "形态转换单单接口";
                    break;
                case "wulApi":
                    msg = "物料档案接口";
                    break;
                case "gysApi":
                    msg = "供应商档案接口";
                    break;
                case "kehuApi":
                    msg = "客户档案接口";
                    break;
                case "plgxccprkapi":
                    msg = "批量更新产成品项目号接口";
                    break;
                case "djshapi":
                    msg = "单据审核接口";
                    break;
                case "wlqdApi":
                    msg = "物料订单接口";
                    break;
                case "ckxxapi":
                    msg = "仓库信息接口";
                    break;
                default:
                    msg = "请检查参数是否正确,没有找到输入参数对应的接口信息！";
                    break;
            }
            try
            {
                //ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                Encoding encoding = Encoding.UTF8;
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);//验证服务器证书回调自动验证
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.Accept = "text/html, application/xhtml+xml, */*";
                request.ContentType = "application/json";

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    msg = msg + "调用成功";
                    return msg;
                }
            }
            catch (Exception ex)
            {
                msg = msg + "调用失败：" + ex.Message;
                return msg;
            }

        }

        public static string HttpPost(string url, string jsonstr, string type, string webApiParam)
        {
            string msg = "";
            switch (webApiParam)
            {
                //查询
                //case "chaxu":
                //    webApiUrl = "http://erp.test.cvming.com/u8sv/ERPU8/Rdrecord01Add";
                //    break;
                //采购入库单接口
                case "cgrkapi":
                    msg = "采购入库单接口";
                    break;
                //采购发票接口
                case "cgfpapi":
                    msg = "采购发票接口";
                    break;
                //销售发票接口
                case "xsfpapi":
                    msg = "销售发票接口";
                    break;
                //发货单接口
                case "fhdapi":
                    msg = "发货单接口";
                    break;
                //材料出库接口
                case "clckapi":
                    msg = "材料出库接口";
                    break;
                //产成品入库接口
                case "ccprkapi":
                    msg = "产成品入库接口";
                    break;
                //其他入库单接口
                case "qtrkdapi":
                    msg = "销售发票接口";
                    break;
                //部门接口
                case "deptApi":
                    msg = "部门档案接口";
                    break;
                //其他出库单接口
                case "qtckdapi":
                    msg = "其他出库单接口";
                    break;
                //调拨单接口
                case "dbdapi":
                    msg = "调拨单接口";
                    break;
                //形态转换单单接口
                case "xtzhdapi":
                    msg = "形态转换单单接口";
                    break;
                case "pingzApi":
                    msg = "凭证单单接口";
                    break;
                case "wulApi":
                    msg = "物料档案接口";
                    break;
                case "gysApi":
                    msg = "供应商档案接口";
                    break;
                case "kehuApi":
                    msg = "客户档案接口";
                    break;
                case "plgxccprkapi":
                    msg = "批量更新产成品项目号接口";
                    break;
                case "djshapi":
                    msg = "单据审核接口";
                    break;
                case "wlqdApi":
                    msg = "物料清单接口";
                    break;
                case "ckxxapi":
                    msg = "仓库信息接口";
                    break;
                default:
                    msg = "请检查参数是否正确,没有找到输入参数对应的接口信息！";
                    break;
            }
            try
            {
                Encoding encoding = Encoding.UTF8;
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);//验证服务器证书回调自动验证
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);//webrequest请求api地址
                request.Accept = "text/html,application/xhtml+xml,*/*";
                request.ContentType = "application/json";
                request.Method = type.ToUpper().ToString();//get或者post
                byte[] buffer = encoding.GetBytes(jsonstr);
                request.ContentLength = buffer.Length;
                request.GetRequestStream().Write(buffer, 0, buffer.Length);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    msg = msg + "调用成功";
                    return msg;
                }
            }

            catch (Exception ex)
            {
                msg = msg + "调用失败：" + ex.Message;
                return msg;
            }
        }


        public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {   // 总是接受  
            return true;
        }


        /// <summary>
        /// 获取指定年月的第一天
        /// </summary>
        /// <param name="year"></param>
        /// <param name="mon"></param>
        /// <returns></returns>

        public static DateTime GetCurMonthFirstDay(string year, string mon)
        {
            DateTime AssemblDate = Convert.ToDateTime(year + "-" + mon + "-" + "01");  // 组装当前指定月份
            return AssemblDate.AddDays(1 - AssemblDate.Day);  // 返回指定当前月份的第一天
        }

        /// <summary>
        /// 获取指定年月的最后一天
        /// </summary>
        /// <param name="year"></param>
        /// <param name="mon"></param>
        /// <returns></returns>

        public static DateTime GetCurMonthLastDay(string year, string mon)
        {
            DateTime AssemblDate = Convert.ToDateTime(year + "-" + mon + "-" + "01");  // 组装当前指定月份
            return AssemblDate.AddDays(1 - AssemblDate.Day).AddMonths(1).AddDays(-1);  // 返回指定当前月份的最后一天
        }
        #endregion
    }
}