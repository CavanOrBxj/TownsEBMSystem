using EBSignature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace TownsEBMSystem
{
   public  class Calcle
    {
         public delegate void MyDelegate();
        // public event MyDelegate MyEvent;

       /// <summary>
       /// 签名函数
       /// </summary>
       /// <param name="pdatabuf"></param>
       /// <param name="datalen"></param>
       /// <param name="random"></param>
       /// <param name="signature"></param>
         public void SignatureFunc(byte[] pdatabuf, int datalen, ref int random, ref byte[] signature)
         {

             try
             {
                 byte[] strSignture = new byte[100];
                 byte[] pucSignature = pdatabuf;
                 if (SingletonInfo.GetInstance().IsUseAddCert)
                 {
                     SingletonInfo.GetInstance().InlayCA.EbMsgSign(pdatabuf, datalen, ref random, ref signature, SingletonInfo.GetInstance().Cert_Index);

                 }
                 else
                 {      //目前暂用平台签名  20180524
                     SingletonInfo.GetInstance().InlayCA.EbMsgSign(pdatabuf, datalen, ref random, ref signature, 2);
                 }
                 string strData = null;
                 for (int i = 0; i < pucSignature.Length; i++)
                 {
                     strData += " " + pucSignature[i].ToString("X2");
                 }
                 string strData2 = null;
                 for (int i = 0; i < signature.Length; i++)
                 {
                     strData2 += " " + signature[i].ToString("X2");
                 }
             }
             catch (Exception ex)
             {
                 
                 throw;
             }
             

         }
    }
}
