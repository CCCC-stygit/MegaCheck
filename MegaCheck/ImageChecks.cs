using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace MegaCheck
{
    class ImageChecks
    {

        public MainWindow.CheckItem CheckImageClipping(PlanSetup ps)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "Image voxels clipping";

            if (ps.StructureSet != null)
            {
                Image image = ps.StructureSet.Image;
                if (image != null)
                {
                    Dose dose = ps.Dose;
                    if (dose != null)
                    {

                        int xMin = 0;
                        int yMin = 0;
                        int zMin = 0;

                        int xMax = image.XSize;
                        int yMax = image.YSize;
                        int zMax = image.ZSize;



                        // Get dose grid dimensions, note that origin refers to the centre of
                        // the min(X,Y,Z) pixel so a 0.5 voxel offset needs to be applied

                        // Get the origin of the dose grid in x,y,z
                        double doseMinX = dose.Origin.x - (0.5 * dose.XRes);
                        double doseMinY = dose.Origin.y - (0.5 * dose.YRes);
                        double doseMinZ = dose.Origin.z - (0.5 * dose.ZRes);
                        // Get the extent of the dose grid in x,y,z
                        double doseMaxX = doseMinX + dose.XSize * dose.XRes - (0.5 * dose.XRes);
                        double doseMaxY = doseMinY + dose.YSize * dose.YRes - (0.5 * dose.YRes);
                        double doseMaxZ = doseMinZ + dose.ZSize * dose.ZRes - (0.5 * dose.ZRes);

                        double imgMinX = image.Origin.x;
                        double imgMinY = image.Origin.y;
                        double imgMinZ = image.Origin.z;

                        double imgMaxX = image.XRes;
                        double imgMaxY = image.YRes;
                        double imgMaxZ = image.ZRes;

                        // Code to get nearest xMin, xMax, .... rounded conservatively to fully encompass all voxels

                        for (int i = 1; i < image.XSize; i++)
                        {
                            if (((imgMinX + i * image.XRes) >= doseMinX) &&
                                ((imgMinX + (i - 1) * image.XRes) <= doseMinX))
                                xMin = i - 1;
                            if (((imgMinX + i * image.XRes) >= doseMaxX) &&
                                ((imgMinX + (i - 1) * image.XRes) <= doseMaxX))
                            {
                                xMax = i;
                                break;
                            }
                        }
                        for (int i = 1; i < image.YSize; i++)
                        {
                            if (((imgMinY + i * image.YRes) >= doseMinY) &&
                                ((imgMinY + (i - 1) * image.YRes) <= doseMinY))
                                yMin = i - 1;
                            if (((imgMinY + i * image.YRes) >= doseMaxY) &&
                                ((imgMinY + (i - 1) * image.YRes) <= doseMaxY))
                            {
                                yMax = i;
                                break;
                            }
                        }
                        for (int i = 1; i < image.ZSize; i++)
                        {
                            if (((imgMinZ + i * image.ZRes) >= doseMinZ) &&
                                ((imgMinZ + (i - 1) * image.ZRes) <= doseMinZ))
                                zMin = i - 1;
                            if (((imgMinZ + i * image.ZRes) >= doseMaxZ) &&
                                ((imgMinZ + (i - 1) * image.ZRes) <= doseMaxZ))
                            {
                                zMax = i;
                                break;
                            }
                        }


                        bool clipping = false;

                        int[,] voxelPlane = new int[image.XSize, image.YSize];
                        for (int z = zMin; z < zMax; z++)
                        {
                            image.GetVoxels(z, voxelPlane);
                            for (int x = xMin; x < xMax; x++)
                            {
                                for (int y = yMin; y < yMax; y++)
                                {

                                    if (voxelPlane[x, y] == 4071)
                                    {
                                        clipping = true;
                                        goto clipping;
                                    }

                                }
                            }
                        }
                        clipping: if (clipping)
                        {
                            check.checkResult = MainWindow.Result.Info;
                            check.checkDetail = "Image voxels are clipping, check strucure overrides are appropriate.";
                        }
                        else
                        {
                            check.checkResult = MainWindow.Result.Pass;
                            check.checkDetail = "No image voxels are clipping.";
                        }
                    }
                    else
                    {
                        check.checkResult = MainWindow.Result.Fail;
                        check.checkDetail = "Dose must be calculated to assess if image voxels are clipping.";
                    }
                }
                
                    else
                    {
                        check.checkResult = MainWindow.Result.Fail;
                        check.checkDetail = "Could not access planning image, does an image exist for this plan?";
                    }
                
            }
            else
            {
                check.checkResult = MainWindow.Result.Fail;
                check.checkDetail = "Could not determine planning CT image because no structure set exists.";
            }

            return check;
        }
    }
}
