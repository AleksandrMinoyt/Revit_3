using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;

namespace Revit_2
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CopyGroup : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            try
            {


                UIDocument uiDoc = commandData.Application.ActiveUIDocument;
                Document doc = uiDoc.Document;


                GroupPickFilter gpf = new GroupPickFilter();
                Reference reference = uiDoc.Selection.PickObject(ObjectType.Element, gpf, "Выберите группу объектов");
                Element element = doc.GetElement(reference);
                Group group = element as Group;

                XYZ groupCenter = GetElementCenter(group);
                Room firstRoom = GetRoomByPoint(doc, groupCenter);

                XYZ roomCenter = GetElementCenter(firstRoom);

                XYZ offset = groupCenter - roomCenter;

                XYZ point = uiDoc.Selection.PickPoint("Выберите точку");

                Room secondRoom = GetRoomByPoint(doc, point);

                XYZ centerSecondRoom = GetElementCenter(secondRoom);




             Transaction trans = new Transaction(doc);
                trans.Start("Копируем группу");
                doc.Create.PlaceGroup(centerSecondRoom+offset, group.GroupType);

                trans.Commit();

            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                message = "Отмена команды";
                return Result.Cancelled;

            }

            catch (Exception ex)
            {

                message = "Ошибка! " + ex.Message;
                return Result.Failed;
            }

            message = "Группа скопирована";
            return Result.Succeeded;


        }

        public XYZ GetElementCenter(Element elem)
        {
            BoundingBoxXYZ bound = elem.get_BoundingBox(null);
            return (bound.Max + bound.Min) / 2;

        }

        public Room GetRoomByPoint(Document doc, XYZ Point)
        {
            FilteredElementCollector fec = new FilteredElementCollector(doc);
            fec.OfCategory(BuiltInCategory.OST_Rooms);
            foreach (Element el in fec)
            {
                Room room = el as Room;
                if (room != null)
                {
                    if (room.IsPointInRoom(Point))
                        return room;
                }
            }
            return null;

        }




    }

    public class GroupPickFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_IOSModelGroups)
                return true;
            else return false;

        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
