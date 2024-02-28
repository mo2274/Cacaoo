using ChocolateDelivery.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.BLL
{
    public class ListBC
    {
        private ChocolateDeliveryEntities context;
        private string logPath;

        public ListBC(ChocolateDeliveryEntities benayaatEntities, string errorLogPath)
        {
            context = benayaatEntities;
            logPath = errorLogPath;
            // context.UpdateEFCon();

        }
        public SM_LISTS? GetList(long list_id)
        {
            var userMenu = new SM_LISTS();
            try
            {
                userMenu = (from o in context.sm_lists
                            where o.List_Id == list_id
                            select o).FirstOrDefault();
            }
            catch (Exception ex)
            {
                globalCls.WriteToFile(logPath, ex.ToString(), true);
                //throw new Exception(ex.ToString());
            }
            return userMenu;
        }

        public List<SM_LIST_FIELDS> GetListFields(long list_id)
        {
            var userMenu = new List<SM_LIST_FIELDS>();
            try
            {
                userMenu = (from o in context.sm_list_fields
                            where o.Show == true
                            && o.List_Id == list_id
                            orderby o.Sequence
                            select o).ToList();
            }
            catch (Exception ex)
            {
                globalCls.WriteToFile(logPath, ex.ToString(), true);
            }
            return userMenu;
        }

        public string getDataField(SM_LIST_FIELDS findDetail, bool? is_StoreProcedure)
        {
            var datafield = "";
            try
            {
                if (is_StoreProcedure == true)
                {
                    datafield = findDetail.Table_Field_Name;
                }
                else
                {
                    if (!string.IsNullOrEmpty(findDetail.Table_Id))
                        datafield = findDetail.Table_Id + "_" + findDetail.Table_Field_Name;
                    else if (findDetail.Table_Field_Name != null && findDetail.Table_Field_Name.ToLower().Contains("as"))
                    {
                        string[] parts = findDetail.Table_Field_Name.Split(new string[] { " as " }, StringSplitOptions.None);
                        datafield = parts[1].Trim();
                    }
                    else
                    {
                        datafield = findDetail.Table_Field_Name;
                    }
                }
            }
            catch (Exception ex)
            {
                globalCls.WriteToFile(logPath, ex.ToString(), true);
            }

            return datafield ?? "";
        }

        public List<SM_LIST_PARAMETERS> GetListParameters(long list_id)
        {
            var userMenu = new List<SM_LIST_PARAMETERS>();
            try
            {
                userMenu = (from o in context.sm_list_parameters
                            where o.Show == true
                            && o.List_Id == list_id
                            select o).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return userMenu;
        }
    }
}
