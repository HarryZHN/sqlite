using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;

class SqliteDB
{
    /// <summary>
    /// 创建数据库文件
    /// </summary>
    /// <param name="fileName">文件名</param>
    public static void CreateDBFile(string fileName)
    {
        string path = Environment.CurrentDirectory + @"/Data/";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        string databaseFileName = path + fileName;
        if (!File.Exists(databaseFileName))
        {
            SQLiteConnection.CreateFile(databaseFileName);
        }
    }
    /// <summary>
    /// 删除数据库
    /// </summary>
    /// <param name="fileName">文件名</param>
    public static void DeleteDBFile(string fileName)
    {
        string path = Environment.CurrentDirectory + @"/Data/" + fileName;
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// 生成连接字符串
    /// </summary>
    /// <returns></returns>
    public static string CreateConnectionString(string dbName)
    {
        SQLiteConnectionStringBuilder connectionString = new SQLiteConnectionStringBuilder();
        connectionString.DataSource = @"data/"+dbName+"";//此处文件名可以使用变量表示

        string conStr = connectionString.ToString();
        return conStr;
    }

    static SQLiteConnection m_dbConnection;
    /// <summary>
    /// 连接到数据库
    /// </summary>
    /// <returns></returns>
    private static SQLiteConnection dbConnection(string dbName)
    {
        m_dbConnection = new SQLiteConnection(CreateConnectionString(dbName));

        m_dbConnection.Open();

        return m_dbConnection;
    }

    /// <summary>
    /// 在指定数据库中创建一个table
    /// </summary>
    /// <param name="sql">sql语言，如：create table highscores (name varchar(20), score int)</param>
    /// <returns></returns>
    public static bool CreateTable(string sql,string dbName)
    {
        try
        {
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection(dbName));
            command.ExecuteNonQuery();
            return true;
        }
        catch (Exception ex)
        {
            Log.ErrorLog("ExecuteNonQuery(" + sql + ")Err:" + ex.Message);
            return false;
        }
        finally
        {
            closeConn();
        }

    }

    /// <summary>
    /// 在指定数据库中删除一个table
    /// </summary>
    /// <param name="tablename">表名称</param>
    /// <returns></returns>
    public static bool DeleteTable(string tablename,string dbName)
    {
        try
        {
            SQLiteCommand cmd = new SQLiteCommand("DROP TABLE IF EXISTS " + tablename, dbConnection(dbName));
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception ex)
        {
            Log.ErrorLog("ExecuteNonQuery(DROP TABLE IF EXISTS " + tablename + ")Err:" + ex);
            return false;
        }
        finally
        {
            closeConn();
        }
    }

    /// <summary>
    /// 在指定表中添加列
    /// </summary>
    /// <param name="tablename">表名</param>
    /// <param name="columnname">列名</param>
    /// <param name="ctype">列的数值类型</param>
    /// <returns></returns>
    public static bool AddColumn(string tablename, string columnname, string ctype,string dbName)
    {
        try
        {
            SQLiteCommand cmd = new SQLiteCommand("ALTER TABLE " + tablename + " ADD COLUMN " + columnname + " " + ctype, dbConnection(dbName));
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception ex)
        {
            Log.ErrorLog("ExecuteNonQuery(ALTER TABLE " + tablename + " ADD COLUMN " + columnname + " " + ctype + ")Err:" + ex);
            return false;
        }
        finally
        {
            closeConn();
        }
    }

    /// <summary>
    /// 执行增删改查操作
    /// </summary>
    /// <param name="sql">查询语言</param>
    /// <returns></returns>
    public static int ExecuteNonQuery(string sql,string dbName)
    {
        try
        {
            SQLiteCommand cmd;
            cmd = new SQLiteCommand(sql, dbConnection(dbName));
            cmd.ExecuteNonQuery().ToString();
            return 1;
        }
        catch (Exception ex)
        {
            Log.ErrorLog("ExecuteNonQuery(" + sql + ")Err:" + ex.Message);
            return 0;
        }
        finally
        {
            closeConn();
        }
    }

    /// <summary>
    /// 返回一条记录查询
    /// </summary>
    /// <param name="sql">sql查询语言</param>
    /// <returns>返回字符串数组</returns>
    public static string[] SqlRow(string sql,string dbName)
    {
        try
        {
            SQLiteCommand sqlcmd = new SQLiteCommand(sql, dbConnection(dbName));//sql语句
            SQLiteDataReader reader = sqlcmd.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }
            string[] Row = new string[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
            {
                Row[i] = (reader[i].ToString());
            }
            reader.Close();
            return Row;
        }
        catch (Exception ex)
        {
            Log.ErrorLog("SqlRow(" + sql + ")Err:" + ex);
            return null;
        }
        finally
        {
            closeConn();
        }
    }

    /// <summary>
    /// 唯一结果查询
    /// </summary>
    /// <param name="sql">sql查询语言</param>
    /// <returns>返回一个字符串</returns>
    public static string sqlone(string sql,string dbName)
    {

        try
        {
            SQLiteCommand sqlcmd = new SQLiteCommand(sql, dbConnection(dbName));//sql语句
            return sqlcmd.ExecuteScalar().ToString();
        }
        catch
        {
            return "";
        }
        finally
        {
            closeConn();
        }
    }

    /// <summary>
    /// 获取一列数据
    /// </summary>
    /// <param name="sql">单列查询</param>
    /// <param name="count">返回结果数量</param>
    /// <returns>返回一个数组</returns>
    public static List<string> sqlcolumn(string sql,string dbName)
    {
        try
        {
            List<string> Column = new List<string>();
            SQLiteCommand sqlcmd = new SQLiteCommand(sql, dbConnection(dbName));//sql语句
            SQLiteDataReader reader = sqlcmd.ExecuteReader();
            while (reader.Read())
            {
                Column.Add(reader[0].ToString());
            }
            reader.Close();
            return Column;
        }
        catch (Exception ex)
        {
            Log.ErrorLog("sqlcolumn(" + sql + ")Err:" + ex);
            return null;
        }
        finally
        {
            closeConn();
        }
    }

    /// <summary>
    /// 返回记录集查询
    /// </summary>
    /// <param name="sql">sql查询语言</param>
    /// <returns>返回查询结果集</returns>
    public static DataTable SqlTable(string sql,string dbName)
    {
        try
        {
            SQLiteCommand sqlcmd = new SQLiteCommand(sql, dbConnection(dbName));//sql语句
            sqlcmd.CommandTimeout = 120;
            SQLiteDataReader reader = sqlcmd.ExecuteReader();
            DataTable dt = new DataTable();
            if (reader != null)
            {
                dt.Load(reader, LoadOption.PreserveChanges, null);
            }
            return dt;
        }
        catch (Exception ex)
        {
            Log.ErrorLog("SqlReader(" + sql + ")Err:" + ex.Message);
            return null;
        }
        finally
        {
            closeConn();
        }
    }
    /// <summary>
    /// 关闭数据库连接
    /// </summary>
    public static void closeConn()
    {
        try
        {
            if (m_dbConnection.State == ConnectionState.Open)
                m_dbConnection.Close();
            else if (m_dbConnection.State == ConnectionState.Broken)
            {
                m_dbConnection.Close();
            }
        }
        catch (Exception ex)
        {
            Log.ErrorLog("closeConnErr:" + ex.Message);
        }
    }

}
