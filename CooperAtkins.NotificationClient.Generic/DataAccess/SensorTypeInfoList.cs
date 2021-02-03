
/*
 *  File Name : SensorTypeInfoList.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 11/23/2010
 */

namespace CooperAtkins.NotificationClient.Generic.DataAccess
{
    using EnterpriseModel.Net;
    using CooperAtkins.Generic; 

    public class SensorTypeInfoList : DomainListBase<SensorTypeInfoList, SensorTypeInfo>
    {

        protected override SensorTypeInfoList LoadList(BaseCriteria criteria)
        {
            try
            {
                Criteria listCriteria = (Criteria)criteria;
                //Initialize the CSqlDbCommand for execute the stored procedure
                CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_GETSENSORTYPEINFOLIST, System.Data.CommandType.Text);

                //Execute reader 
                CDAO.ExecReader(cmd);
                while (CDAO.DataReader.Read())
                {
                    SensorTypeInfo sensorTypeInfo = new SensorTypeInfo()
                    {
                        ID = CDAO.DataReader["RecID"].ToInt(),
                        Offset1 = CDAO.DataReader["Offset1"].ToDouble(),
                        Offset2 = CDAO.DataReader["Offset2"].ToDouble(),
                        Scale1 = CDAO.DataReader["Scale1"].ToDouble(1),
                        Scale2 = CDAO.DataReader["Scale2"].ToDouble(1),
                        UOM = CDAO.DataReader["UOM"].ToStr(),
                        Description = CDAO.DataReader["Description"].ToStr(CDAO.DataReader["SensorType"].ToStr()),
                        SensorType = CDAO.DataReader["SensorType"].ToStr(),
                        SubType = CDAO.DataReader["SubType"].ToStr(),
                        nDecimals = CDAO.DataReader["nDecimals"].ToInt(),
                        isTemp = CDAO.DataReader["isTemp"].ToBoolean()
                    };

                    this.Add(sensorTypeInfo);
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                CDAO.CloseDataReader();
                CDAO.Dispose();
            }

            return this;
        }
    }
}
