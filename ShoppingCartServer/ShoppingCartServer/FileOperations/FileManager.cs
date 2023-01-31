using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using ShoppingCartServer.Models;
using ShoppingCartServer.Utils;

namespace ShoppingCartServer.FileOperations;

public abstract class FileManager
{
    protected string customersPathFile;
    protected string productsPathFile;
    protected string shoppingHistoryPathFile;
    
    //separators
    protected const string s1 = ";";
    protected const string s2 = "#";
    protected const string s3 = "$";

    protected FileManager(string customersPathFile, string productsPathFile, string shoppingHistoryPathFile)
    {
        this.customersPathFile = customersPathFile;
        this.productsPathFile = productsPathFile;
        this.shoppingHistoryPathFile = shoppingHistoryPathFile;
    }
}