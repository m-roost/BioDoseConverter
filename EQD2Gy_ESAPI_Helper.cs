using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace Lib_EQD2Gy_Helper
{
   
    public static class EQD2Gy_ESAPI_Helper
    {
        /// <summary>
        /// Copy Dose from sourcePlan to targetPlan, make sure the absolute dose is the same, while keeping the same Rx and # of fractions.
        /// </summary>
        public static EvaluationDose Copy_EvaluationDose_n_MakeSure_Identical(this ExternalPlanSetup targetPlan, PlanSetup sourcePlan)
        {
            Console.WriteLine($"\n-------- Copy Dose from {sourcePlan.Id} to {targetPlan.Id}, keep the same absolute dose value --------");

            int nf_new = sourcePlan.NumberOfFractions ?? 0;

            targetPlan.SetPrescription(nf_new, sourcePlan.DosePerFraction, 1.0f);

            var evalDose = targetPlan.CopyEvaluationDose(sourcePlan.Dose);

            DoseValue dose_at_voxel_1_eval = evalDose.VoxelToDoseValue(1);

            DoseValue dose_at_voxel_1_origin = sourcePlan.Dose.VoxelToDoseValue(1);

            if (dose_at_voxel_1_eval.Unit != DoseValue.DoseUnit.Percent ||
               dose_at_voxel_1_origin.Unit != DoseValue.DoseUnit.Percent)
            {
                throw new Exception("Dose at 1 voxel in evalDose and/or sourcePlan is not in the unit of Percent. This function cannot handle this situation. Please contect the developer to adapt the code to your environment.");
            }

            Console.WriteLine($"Dose at voxel 1 (origin): {dose_at_voxel_1_origin.Dose} {dose_at_voxel_1_origin.UnitAsString}");
            Console.WriteLine($"Dose at voxel 1 (eval)  : {dose_at_voxel_1_eval.Dose} {dose_at_voxel_1_eval.UnitAsString}");

            double dose_scale_rate = dose_at_voxel_1_origin.Dose / dose_at_voxel_1_eval.Dose;

            Console.WriteLine($"Dose scale rate: {dose_scale_rate}");

            var Rx_orig = sourcePlan.TotalDose;
            var Rx_eval = targetPlan.TotalDose;

            Console.WriteLine($"Total Dose (origin): {Rx_orig}");
            Console.WriteLine($"Total Dose (eval)  : {Rx_eval}");


            int xSize = evalDose.XSize;
            int ySize = evalDose.YSize;
            int zSize = evalDose.ZSize;


            for (int z = 0; z < zSize; z++)
            {
                int[,] plane = new int[xSize, ySize];

                evalDose.GetVoxels(z, plane);

                for (int x = 0; x < xSize; x++)
                {
                    for (int y = 0; y < ySize; y++)
                    {
                        if (plane[x, y] == 0) continue;

                        plane[x, y] = (int)(plane[x, y] * dose_scale_rate);
                    }
                }

                evalDose.SetVoxels(z, plane);
            }

            Console.WriteLine("---------- Copy dose complete ------------");

            return evalDose;
        }



        /// <summary>
        /// Copy Dose from sourcePlan to targetPlan; adjust dose to EQD2Gy, set # of Fractions to 1 and Rx to total EQD2Gy in target plan.
        /// Return the EvaluationDose from target plan 
        /// </summary>
        public static EvaluationDose Copy_EvaluationDose_n_adjust_to_EQD2Gy(this ExternalPlanSetup targetPlan, PlanSetup sourcePlan, double alpha_beta)
        {
            Console.WriteLine($"\n-------- Calculate EQD2Gy Dose from {sourcePlan.Id} to {targetPlan.Id} --------");

            var evalDose = targetPlan.CopyEvaluationDose(sourcePlan.Dose);

            DoseValue dose_at_voxel_1_eval = evalDose.VoxelToDoseValue(1);

            DoseValue dose_at_voxel_1_origin = sourcePlan.Dose.VoxelToDoseValue(1);

            if (dose_at_voxel_1_eval.Unit != DoseValue.DoseUnit.Percent ||
               dose_at_voxel_1_origin.Unit != DoseValue.DoseUnit.Percent)
            {
                throw new Exception("Dose at 1 voxel in evalDose and/or sourcePlan is not in the unit of Percent. This function cannot handle this situation. Please contect the developer to adapt the code to your environment.");
            }


            int n1 = (int)sourcePlan.NumberOfFractions;

            double Rx1 = sourcePlan.TotalDose.Dose;
            var Rx1_unit = sourcePlan.TotalDose.Unit;
            
            if (Rx1_unit != DoseValue.DoseUnit.Gy && Rx1_unit != DoseValue.DoseUnit.cGy)
            {
                throw new Exception("Total dose in source plan is not in the unit of Gy or cGy. This function cannot handle this situation.");
            }

            double Rx1_Gy;

            if (Rx1_unit == DoseValue.DoseUnit.cGy)
            {
                Rx1_Gy = Rx1 / 100.0; // convert cGy to Gy
                Console.WriteLine("\n! --- Converted Rx1 unit from cGy to Gy ---\n");
            }
            else
            {
                Rx1_Gy = Rx1;
            }


            double Rx2_Gy = Rx1_Gy * (alpha_beta + Rx1_Gy / n1) / (alpha_beta + 2.0f);

            Console.WriteLine($"Rx1 is {Rx1} {Rx1_unit} with {n1} fractions ---- EQD2Gy Rx2 is {Rx2_Gy} Gy; with alpha/beta = {alpha_beta}");

            double r1 = dose_at_voxel_1_origin.Dose;
            double r2 = dose_at_voxel_1_eval.Dose;

            Console.WriteLine($"Dose at voxel 1 (origin): {r1} {dose_at_voxel_1_origin.UnitAsString}");
            Console.WriteLine($"Dose at voxel 1 (eval)  : {r2} {dose_at_voxel_1_eval.UnitAsString}");


            if (n1 <= 0)
            {
                throw new Exception("Number of fractions in source plan is less than or equal to 0. This function cannot handle this situation.");
            }

            if (Rx1_Gy <= 0 || Rx2_Gy <= 0)
            {
                throw new Exception("Total dose in source plan is less than or equal to 0. This function cannot handle this situation.");
            }

            if(Rx1_unit == DoseValue.DoseUnit.cGy) 
            {
                targetPlan.SetPrescription(n1, new DoseValue(Rx2_Gy / n1 * 100, DoseValue.DoseUnit.cGy), 1.0f);
            }
            else
            {
                // Set targetPlan Rx in Gy unit, don't know how this would behave in a cGy system yet.
                targetPlan.SetPrescription(n1, new DoseValue(Rx2_Gy / n1, DoseValue.DoseUnit.Gy), 1.0f); 
            }


            int xSize = evalDose.XSize;
            int ySize = evalDose.YSize;
            int zSize = evalDose.ZSize;

            for (int z = 0; z < zSize; z++)
            {
                int[,] plane_source = new int[xSize, ySize];
                int[,] plane_target = new int[xSize, ySize];

                sourcePlan.Dose.GetVoxels(z, plane_source);

                for (int x = 0; x < xSize; x++)
                {
                    for (int y = 0; y < ySize; y++)
                    {
                        if (plane_source[x, y] == 0) continue;

                        var v1 = plane_source[x, y];

                        var v2 = calculate_EQD2Gy_at_one_voxel(v1, r1, Rx1_Gy, n1, alpha_beta, Rx2_Gy, r2);

                        plane_target[x, y] = v2;
                    }
                }

                evalDose.SetVoxels(z, plane_target);
            }

            Console.WriteLine("---------- Copy and calculate EQD2Gy dose complete ------------");

            return evalDose;
        }



        private static int calculate_EQD2Gy_at_one_voxel(int v1, double r1, double Rx1_Gy, int n1, double alpha_beta, double Rx2_Gy, double r2)
        {
            double p1 = v1 * r1;

            double d1 = p1 / 100 * Rx1_Gy;

            double d2 = d1 * (alpha_beta + d1 / n1) / (alpha_beta + 2);

            double p2 = d2 / Rx2_Gy * 100;

            int v2 = (int)(p2 / r2);

            return v2;
        }




        public static void printDoseInfo(PlanSetup ps)
        {
            Console.WriteLine($"\n======================= {ps.Id} ===== {ps.NumberOfFractions} fractions ===== {ps.DosePerFraction.Dose} {ps.DosePerFraction.UnitAsString} per/fraction ====== TotalDose {ps.TotalDose.Dose} {ps.TotalDose.UnitAsString} =========");

            var evalDose = ps.Dose;

            double dose_at_voxel_1 = evalDose.VoxelToDoseValue(1).Dose;

            DoseValue.DoseUnit dose_unit_at_voxel_1 = evalDose.VoxelToDoseValue(1).Unit;

            Console.WriteLine($"Dose at voxel 1: {dose_at_voxel_1} {dose_unit_at_voxel_1}");

            int xSize = evalDose.XSize;
            int ySize = evalDose.YSize;
            int zSize = evalDose.ZSize;

            Console.WriteLine($"xSize {xSize}, ySize {ySize}, zSize {zSize}");

            Console.WriteLine($"TotalDose {ps.TotalDose.Dose} {ps.TotalDose.UnitAsString}");


            //int z_layer = zSize / 2;

            int max_voxel = 0;
            (int x, int y, int z) max_voxel_coords = (0, 0, 0);


            for (int z = 0; z < zSize; z++)
            {
                int[,] plane = new int[xSize, ySize];

                evalDose.GetVoxels(z, plane);

                for (int x = 0; x < xSize; x++)
                {
                    for (int y = 0; y < ySize; y++)
                    {

                        if (plane[x, y] == 0) continue;


                        if (plane[x, y] > max_voxel)
                        {
                            max_voxel = plane[x, y];
                            max_voxel_coords = (x, y, z);
                        }

                        //var vii_dose = evalDose.VoxelToDoseValue(plane[x, y]);

                        //vii_dose.DoseAsUnit()
                        //if (vii_dose.Dose < 0.99) continue;

                        //Console.WriteLine($"plan[{x},{y},{z}] = [{plane[x, y]}] Voxel {vii_dose.Dose} {vii_dose.UnitAsString}");
                    }
                }
            }

            Console.WriteLine($"Max voxel value: {max_voxel} with {evalDose.VoxelToDoseValue(max_voxel).Dose} {evalDose.VoxelToDoseValue(max_voxel).UnitAsString} at coordinates {max_voxel_coords}");

            double px = evalDose.Origin.x + max_voxel_coords.x * evalDose.XRes;
            double py = evalDose.Origin.y + max_voxel_coords.y * evalDose.YRes;
            double pz = evalDose.Origin.z + max_voxel_coords.z * evalDose.ZRes;

            Console.WriteLine($"Max voxel physical coordinates: ({px}, {py}, {pz})");

            Console.WriteLine($"x y z resolution: ({evalDose.XRes}, {evalDose.YRes}, {evalDose.ZRes})");

            var doseAtPoint = evalDose.GetDoseToPoint(new VVector(px, py, pz));

            Console.WriteLine($"Dose at max voxel physical coordinates: {doseAtPoint.Dose} {doseAtPoint.UnitAsString}");

        }
    }
}

