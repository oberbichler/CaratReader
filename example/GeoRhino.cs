using Carat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example
{
    class GeoRhino
    {
        public static GeoRhino Load(string path)
        {
            var geoRhino = new GeoRhino();

            var reader = CaratReader.FromFile(path);

            while (!reader.EOF)
            {
                if (geoRhino.TryReadNdCoor(reader))
                    continue;

                if (geoRhino.TryReadCtrlPtsParDef(reader))
                    continue;

                if (geoRhino.TryReadNurbsPatch(reader))
                    continue;

                if (geoRhino.TryReadCtrlPtsNodes(reader))
                    continue;

                if (geoRhino.TryReadBrep(reader))
                    continue;

                if (geoRhino.TryReadNurbsPatchPar(reader))
                    continue;

                if (geoRhino.TryReadCtrlPtsPar(reader))
                    continue;

                if (geoRhino.TryReadGpPointGeo(reader))
                    continue;

                if (geoRhino.TryReadEvalPoint(reader))
                    continue;

                if (geoRhino.TryReadCoupPointGeo(reader))
                    continue;

                if (geoRhino.TryReadDeEleProp(reader))
                    continue;

                if (reader.Match("end"))
                    continue;

                throw reader.NewUnexpectedTokenException();
            }

            return geoRhino;
        }

        private bool TryReadNdCoor(CaratReader reader)
        {
            if (!reader.Match("ND-COOR"))
                return false;

            // TODO: process data
            Console.WriteLine($"ND-COOR");

            while (!reader.EOF)
            {
                if (!reader.Match("NODE"))
                    break;

                var id = reader.ReadInteger();

                var x = default(double);
                var y = default(double);
                var z = default(double);

                while (!reader.EOF)
                {
                    if (reader.Match("X"))
                    {
                        x = reader.ReadDouble();
                        continue;
                    }

                    if (reader.Match("Y"))
                    {
                        y = reader.ReadDouble();
                        continue;
                    }

                    if (reader.Match("Z"))
                    {
                        z = reader.ReadDouble();
                        continue;
                    }

                    break;
                }

                // TODO: process data
                Console.WriteLine($"  NODE ID {id} X {x} Y {y} Z {z}");
            }

            return true;
        }

        private bool TryReadCtrlPtsParDef(CaratReader reader)
        {
            if (!reader.Match("CTRL_PTS_PAR_DEF"))
                return false;

            // TODO: process data
            Console.WriteLine($"CTRL_PTS_PAR_DEF");

            while (!reader.EOF)
            {
                if (!reader.Match("CTRLPT_PAR"))
                    break;

                var id = reader.ReadInteger();

                var x = reader.ReadDouble();
                var y = reader.ReadDouble();
                var z = reader.ReadDouble();

                // TODO: process data
                Console.WriteLine($"  CTRLPT_PAR ID {id} X {x} Y {y} Z {z}");
            }

            return true;
        }

        private bool TryReadNurbsPatch(CaratReader reader)
        {
            if (!reader.Match("NURBS_PATCH"))
                return false;

            var id = reader.ReadInteger();

            reader.Expect(":");

            var nctrl = default(int?);
            var mctrl = default(int?);
            var pdeg = default(int?);
            var qdeg = default(int?);
            var uknot = default(List<double>);
            var vknot = default(List<double>);
            var trimmingID = default(int?);
            var addPar = default(int?);

            if (reader.Match("NURBS_2D"))
            {
                while (!reader.EOF)
                {
                    if (reader.Match("CTRL_PTS"))
                    {
                        reader.Expect("=");
                        reader.Expect("CTRL_PTS_NODES");
                        var ctrl_pts_nodes = reader.ReadInteger();
                        continue;
                    }

                    if (reader.Match("NCTRL"))
                    {
                        if (nctrl != null)
                            throw reader.NewDuplicateTokenException();

                        reader.Expect("=");
                        nctrl = reader.ReadInteger();
                        continue;
                    }

                    if (reader.Match("MCTRL"))
                    {
                        if (mctrl != null)
                            throw reader.NewDuplicateTokenException();

                        reader.Expect("=");
                        mctrl = reader.ReadInteger();
                        continue;
                    }

                    if (reader.Match("PDEG"))
                    {
                        if (pdeg != null)
                            throw reader.NewDuplicateTokenException();

                        reader.Expect("=");
                        pdeg = reader.ReadInteger();
                        continue;
                    }

                    if (reader.Match("QDEG"))
                    {
                        if (qdeg != null)
                            throw reader.NewDuplicateTokenException();

                        reader.Expect("=");
                        qdeg = reader.ReadInteger();
                        continue;
                    }

                    if (reader.Match("UKNOT"))
                    {
                        if (uknot != null)
                            throw reader.NewDuplicateTokenException();

                        reader.Expect("=");
                        uknot = reader.ReadDoubleList();
                        continue;
                    }

                    if (reader.Match("VKNOT"))
                    {
                        if (vknot != null)
                            throw reader.NewDuplicateTokenException();

                        reader.Expect("=");
                        vknot = reader.ReadDoubleList();
                        continue;
                    }

                    if (reader.Match("TRIMMING"))
                    {
                        if (trimmingID != null)
                            throw reader.NewDuplicateTokenException();

                        reader.Expect("=");
                        reader.Expect("B_REP");
                        trimmingID = reader.ReadInteger();
                        continue;
                    }

                    if (reader.Match("ADD_PAR"))
                    {
                        if (addPar != null)
                            throw reader.NewDuplicateTokenException();

                        reader.Expect("=");
                        addPar = reader.ReadInteger();
                        continue;
                    }

                    break;
                }
            }
            else
            {
                throw reader.NewUnexpectedTokenException();
            }

            // TODO: process data
            Console.WriteLine($"NURBS_PATCH ID {id} NCTRL {nctrl} MCTRL {mctrl} ...");

            return true;
        }

        private bool TryReadCtrlPtsNodes(CaratReader reader)
        {
            if (!reader.Match("CTRL_PTS_NODES"))
                return false;

            var id = reader.ReadInteger();

            // TODO: process data
            Console.WriteLine($"CTRL_PTS_NODES ID {id}");

            while (!reader.EOF)
            {
                if (reader.Match("NODE_ID"))
                {
                    var nodeId = reader.ReadInteger();
                    reader.Expect("W");
                    var w = reader.ReadDouble();

                    // TODO: process data
                    Console.WriteLine($"  NODE_ID ID {id} WEIGHT {w}");

                    continue;
                }

                break;
            }

            return true;
        }

        private bool TryReadBrep(CaratReader reader)
        {
            if (!reader.Match("B_REP"))
                return false;

            var id = reader.ReadInteger();

            while (!reader.EOF)
            {
                if (reader.Match("B_REP_LOOP"))
                {
                    var loopId = reader.ReadInteger();
                    var loopType = reader.ReadInteger();

                    // TODO: process data
                    Console.WriteLine($"B_REP_LOOP ID {loopId} TYPE {loopType}");

                    while (!reader.EOF)
                    {
                        if (reader.TryRead(out int trimId))
                        {
                            var orientation = reader.ReadBoolean();

                            // TODO: process data
                            Console.WriteLine($"  TRIM ID {trimId} ORIENTATION {orientation}");

                            continue;
                        }

                        break;
                    }

                    continue;
                }

                break;
            }

            return true;
        }

        private bool TryReadNurbsPatchPar(CaratReader reader)
        {
            if (!reader.Match("NURBS_PATCH_PAR"))
                return false;

            var id = reader.ReadInteger();

            reader.Expect(":");

            var ctrlPtsNodes = default(int?);
            var nctrl = default(int?);
            var pdeg = default(int?);
            var uknot = default(List<double>);

            if (reader.Match("NURBS_1D"))
            {
                while (!reader.EOF)
                {
                    if (reader.Match("CTRL_PTS"))
                    {
                        reader.Expect("=");
                        reader.Expect("CTRL_PTS_PAR");
                        ctrlPtsNodes = reader.ReadInteger();
                        continue;
                    }

                    if (reader.Match("NCTRL"))
                    {
                        reader.Expect("=");
                        nctrl = reader.ReadInteger();
                        continue;
                    }

                    if (reader.Match("PDEG"))
                    {
                        reader.Expect("=");
                        pdeg = reader.ReadInteger();
                        continue;
                    }

                    if (reader.Match("UKNOT"))
                    {
                        reader.Expect("=");
                        uknot = reader.ReadDoubleList();
                        continue;
                    }

                    break;
                }
            }
            else
            {
                throw reader.NewUnexpectedTokenException();
            }

            // TODO: process data
            Console.WriteLine($"NURBS_PATCH_PAR ID {id} ...");

            return true;
        }

        private bool TryReadCtrlPtsPar(CaratReader reader)
        {
            if (!reader.Match("CTRL_PTS_PAR"))
                return false;

            var id = reader.ReadInteger();

            // TODO: process data
            Console.WriteLine($"CTRL_PTS_PAR ID {id}");

            while (!reader.EOF)
            {
                if (reader.Match("CTRLPT_PAR_ID"))
                {
                    var ctrlptParId = reader.ReadInteger();

                    // TODO: process data
                    Console.WriteLine($"  CTRLPT_PAR_ID ID {ctrlptParId}");

                    continue;
                }

                break;
            }

            return true;
        }

        private bool TryReadGpPointGeo(CaratReader reader)
        {
            if (!reader.Match("GP_POINT_GEO"))
                return false;

            var value = reader.ReadInteger();
            var u = reader.ReadDouble();
            var v = reader.ReadDouble();

            // TODO: process data
            Console.WriteLine($"GP_POINT_GEO U {u} V {v} ...");

            return true;
        }

        private bool TryReadEvalPoint(CaratReader reader)
        {
            if (!reader.Match("EVAL_POINT"))
                return false;

            var value1 = reader.ReadInteger();
            var value2 = reader.ReadInteger();
            var u = reader.ReadDouble();
            var v = reader.ReadDouble();

            // TODO: process data
            Console.WriteLine($"EVAL_POINT U {u} V {v} ...");

            return true;
        }

        private bool TryReadCoupPointGeo(CaratReader reader)
        {
            if (!reader.Match("COUP_POINT_GEO"))
                return false;

            var value = reader.ReadInteger();
            var u = reader.ReadDouble();
            var v = reader.ReadDouble();

            // TODO: process data
            Console.WriteLine($"COUP_POINT_GEO U {u} V {v} ...");

            return true;
        }

        private bool TryReadDeEleProp(CaratReader reader)
        {
            if (!reader.Match("DE_ELE_PROP"))
                return false;

            var id = reader.ReadInteger();
            var youngsmodulus = reader.ReadDouble();
            var poisonsratio = reader.ReadDouble();
            var alpha = reader.ReadDouble();
            var density = reader.ReadDouble();
            var type = reader.ReadString();
            var thickness = reader.ReadDouble();

            // TODO: process data
            Console.WriteLine($"DE_ELE_PROP ID {id} ...");

            return true;
        }
    }
}
