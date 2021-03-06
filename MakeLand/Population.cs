﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.Media;
using System.Threading.Tasks;

namespace MakeLand
{
    public class Population
    {
        public int generation = 0;
        public int bestScore = 0;
        public int bestIndex = 0;
        public int numInPop = 0;

        public int[] listOfLiving;
        public int countOfLiving;

        public Phenotype[] maps = null;

        public Population(int numInPopZ, Random r)
        {
            numInPop = numInPopZ;
            maps = new Phenotype[numInPop];
            for (int i = 0; i < numInPop; i++)
            {
                Genotype g = new Genotype(r);
                Phenotype p = new Phenotype(g,0);
                p.createPheno();
                p.setScore();
                maps[i] = p; 
            }
        }

        /// <summary>
        /// Returns the index of the best individual and updates bestScore
        /// </summary>
        /// <returns></returns>
        public int findBest()
        {
            Phenotype p = maps[0];
            bestScore = p.score;
            bestIndex = 0;
            for (int i = 1; i < numInPop; i++)
            {
                p = maps[i];
                if (p.score > bestScore)
                {
                    bestIndex = i;
                    bestScore = p.score;
                }
            }
            return bestIndex;
        }


        /// <summary>
        /// Finds the worst individual thats actually alive
        /// </summary>
        /// <returns></returns>
        public int findWorstAlive()
        {
            bool first = true;
            int worstScore = 0;
            int worstIndex = 0;

            for (int i = 1; i < numInPop; i++)
            {
                Phenotype p = maps[i];
                if (p.alive && first)
                {
                    first = false;
                    worstScore = p.score;
                    worstIndex = i;
                    continue;
                }

                if (p.alive && p.score < worstScore)
                {
                    worstScore = p.score;
                    worstIndex = i;
                }
            }
            return worstIndex;
        }

        /// <summary>
        /// Just a standard getter
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Phenotype getPhenotype(int i)
        {
            return maps[i];
        }

        /// <summary>
        /// Unsets the newborn flag for the entire population
        /// </summary>
        public void unsetNewborn()
        {
            for (int i = 1; i < numInPop; i++)
            {
                getPhenotype(i).newborn = false;
            }
        }

        /// <summary>
        /// Kills the weakest
        /// </summary>
        /// <param name="n"></param>
        public void killThisMany(int n)
        {

            for (int i = 0; i <n; i++)
            {
                int k = findWorstAlive();
                getPhenotype(k).alive = false;
            }
        }



        /// <summary>
        /// Search for dead individuals - replace them with living newborn ones
        /// </summary>
        public void breedPopulation(Random r)
        {
            listOfLiving = new int[Params.populationCnt];
            countOfLiving=0;
            for (int i = 0; i < Params.populationCnt; i++)
            {
                if (getPhenotype(i).alive && (!getPhenotype(i).newborn))
                {
                    listOfLiving[i] = i;
                    countOfLiving++;
                }
            }

            for (int i = 0; i < Params.populationCnt; i++)
            {
                if (!getPhenotype(i).alive)
                {
                    int mum = r.Next(0, countOfLiving);
                    int dad = r.Next(0, countOfLiving);
                    mum = listOfLiving[mum];
                    dad = listOfLiving[dad];
                    Phenotype mumP = getPhenotype(mum);
                    Phenotype dadP = getPhenotype(dad);
                    Genotype ggg = makeGenome(mumP.genotype,dadP.genotype);
                    if (Params.mutationPercent > r.Next(0, 100))
                    {
                        ggg = mutate(ggg, r);
                    }
                    checkDuplicateGenes(ggg);
                    maps[i] = new Phenotype(ggg, G.pop.generation);

                }
            }
            if (generation % Params.checkDuplicateGenomes == 0)
            {
                checkDuplicateGenotypes();
            }
        }


        public bool checkDuplicateGenes(Genotype ggg)
        {
            bool retv = false;
            for (int i = 0; i < Params.genotypeSize; i++)
                for (int k = i+1; k < Params.genotypeSize; k++)
                {
                    if(ggg.genes[i].equal(ggg.genes[k]))
                      {
                        G.dupGeneCount++;
                        ggg.genes[i] = new Gene(G.rnd);
                        retv = true;
                      }
                }
            return retv;
        }

        public Genotype mutate(Genotype g, Random r)
        {
            G.mutationCount++;
            // How many to modify?
            int genesToModify = r.Next(g.genes.Length);
            int[] whichGenes = new int[genesToModify];
            // Which to modify?
            for (int i = 0; i < genesToModify; i++)
            {
                whichGenes[i] = r.Next(g.genes.Length);
            }
            // Do the modification
            for (int i = 0; i < genesToModify; i++)
            {
                int geneIndex = whichGenes[i];
                int type = r.Next(13);
                switch (type)
                {
                    case 0:
                    case 1:
                        // roll new gene
                        g.genes[geneIndex] = new Gene(r);
                        break;
                    case 2:
                    case 3:
                        // new random x
                        g.genes[geneIndex].x = r.Next(0, Params.dimX);
                        break;
                    case 4:
                    case 5:
                        // new random y
                        g.genes[geneIndex].y = r.Next(0, Params.dimY);
                        break;
                    case 6:
                    case 7:
                        // new terrain
                        g.genes[geneIndex].terrain = (byte) r.Next(0, 3);
                        break;
                    case 8:
                    case 9:
                        // new repeatx
                        g.genes[geneIndex].repeatX = r.Next(0, Params.maxRepeat);
                        break;
                    case 10:
                    case 11:
                        // new repeaty
                        g.genes[geneIndex].repeatY = r.Next(0, Params.maxRepeat);
                        break;
                    case 12:
                        // roll a new genome
                        g = new Genotype(r);
                        break;
                }
            }
            return g;
        }

        /// <summary>
        /// create a new geneome from mum and dad
        /// </summary>
        /// <param name="g1"></param>
        /// <param name="g2"></param>
        /// <returns></returns>
        public Genotype makeGenome(Genotype g1, Genotype g2)
        {
            Genotype retv = new Genotype();
            for (int i = 0; i < Params.genotypeSize; i++)
            {
                if (G.rnd.NextDouble()<0.5)
                {
                    retv.genes[i] = new Gene(g1.genes[i]);
                }
                else
                {
                    retv.genes[i] = new Gene(g2.genes[i]);
                }
            }
            return retv;
        }

        public void checkDuplicateGenotypes()
        {
            for (int i = 0; i < Params.populationCnt; i++)
            {
                Genotype g = getPhenotype(i).genotype;
                if (checkDuplicateGenes(g)) continue;
                for (int k = i + 1; k < Params.populationCnt; k++)
                {
                    Genotype kk = getPhenotype(k).genotype;
                    if (kk.equal(g))
                    {
                        g = mutate(g, G.rnd);
                        G.dupGeneomeCount++;
                    }
                }
            }
        }


        /// <summary>
        /// what it sounds like
        /// </summary>
        public void do1Generation()
        {
            G.pop.generation++;
            unsetNewborn();
            killThisMany(Params.populationCnt / 2);
            breedPopulation(G.rnd);
            //if (Params.checkDuplicateGenomes != -1 && G.pop.generation % Params.checkDuplicateGenomes == 0) checkDuplicateGenotypes(); 
            Application.DoEvents();
        }

    }

    public class Genotype
    {
        public Gene[] genes = new Gene[Params.genotypeSize];

        public Genotype(Random r)
        {
            for (int i = 0; i < Params.genotypeSize; i++)
                genes[i] = new Gene(r);
        }

        public Genotype()
        {
            for (int i = 0; i < Params.genotypeSize; i++)
                genes[i] = new Gene();
        }

        public bool equal(Genotype gg)
        {
            for (int i = 0; i < Params.genotypeSize; i++)
            {
                if (!(gg.genes[i].equal(genes[i]))) return false;
            }
            return true;
        }
    }



    public class Gene
    {
        public byte terrain=0;
        public int x=0;
        public int y=0;
        public int repeatY = 0;
        public int repeatX = 0;

        public Gene()
        {

        }

        public Gene(byte ter, int xx, int yy, int rptX, int rptY)
        {
            terrain = ter;
            x = xx;
            y = yy;
            repeatX = rptX;
            repeatY = rptY;
        }

        /// <summary>
        /// New Random Gene
        /// </summary>
        /// <param name="r"></param>
        public Gene(Random r)
        {
            terrain = (byte) r.Next(0,3);
            x = r.Next(0, Params.dimX);
            y = r.Next(0, Params.dimY);
            repeatX = r.Next(0, Params.maxRepeat);
            repeatY = r.Next(0, Params.maxRepeat);
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="gg"></param>
        public Gene(Gene gg) // copy constructor
        {
            terrain = gg.terrain;
            x = gg.x;
            y = gg.y;
            repeatX = gg.repeatX;
            repeatY = gg.repeatY;
        }

        public bool equal(Gene g)
        {
            if (g.x != x) return false;
            if (g.y != y) return false;
            if (g.repeatX != repeatX) return false;
            if (g.repeatY != repeatY) return false;
            if (g.terrain != terrain) return false;
            return true;
        }

    }

    public class Phenotype
    {
        public Genotype genotype = null; // reference class - this is a pointer not a copy
        byte[,] pheno = null;
        Bitmap bitm = null;
        public int score = 0;
        public bool alive = true;
        public bool newborn = true;
        public int gen = 0;

        /// <summary>
        /// Default constructor probably not helpfull
        /// </summary>
        public Phenotype()
        {
            // default is all null - no need for code yet
        }

        /// <summary>
        /// This is the critical constructor it creates the pheno array for scoring
        /// </summary>
        /// <param name="gg"></param>
        public Phenotype(Genotype gg, int generationCount)
        {
            genotype = gg;
            createPheno();
            setScore();
            gen = generationCount;

        }

        /// <summary>
        ///  create the pheno array
        /// </summary>
        public void createPheno()
        {
            pheno = new byte[Params.dimX, Params.dimY];
            for (int x = 0; x < Params.dimX; x++)
                for (int y = 0; y < Params.dimY; y++) { pheno[x, y] = 0; } // initialise to 0

            for (int i = 0; i < Params.genotypeSize; i++)
            {
                Gene g = genotype.genes[i];
                for (int kx = 0; kx < g.repeatX; kx++)
                    for (int ky = 0; ky < g.repeatY; ky++)
                    {
                        int x = g.x + kx;
                        int y = g.y + ky;
                        if (y < Params.dimY && x < Params.dimX) pheno[x, y] = g.terrain;
                    }

            }
        }

        public int getTerrainSafe(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Params.dimX || y >= Params.dimY) return 0;
            return pheno[x, y];
        }

        /// <summary>
        /// returns the score for selection - also stores it in Phenotype
        /// </summary>
        /// <returns></returns>
        public int setScore()
        {
            score = 0;

            int seaCount = 0;
            int landCount = 0;
            int mountainCount = 0;

            Object thisLock = new Object();

            ParallelLoopResult loopResult = Parallel.For(0, Params.dimX, x =>
            {
                for (int y = 0; y < Params.dimY; y++)
                {
                    switch (pheno[x, y])
                    {
                        case 0: // sea

                            int tempSea = scoreSea(x, y);
                            lock (thisLock)
                            {
                                seaCount++;
                                score += tempSea;
                            }
                            break;
                        case 1: // land
                            
                            int tempLand = scoreLand(x, y);
                            lock (thisLock)
                            {
                                landCount++;
                                score += tempLand;
                            }
                            break;
                        case 2: // mountains
                            int tempMountain = scoreMountains(x, y);
                            lock (thisLock)
                            {
                                mountainCount++;
                                score += tempMountain;
                            }
                            
                            break;
                    }
                }
            });

            float totalCount = landCount + seaCount + mountainCount;

            float nearEnough = 0.5f;

            // Adjust score depending on how close to target land count it is
            int targetLandCount = (int) (totalCount * 0.5f);
            int landDiff = Math.Abs(targetLandCount - landCount);
            if (landDiff > totalCount * nearEnough)
            {
                score -= landDiff;
            }

            // Adjust score depending on how close to target mountains count it is
            int targetMountainsCount = (int)(totalCount * 0.15f);
            int mountainDiff = Math.Abs(targetMountainsCount - mountainCount);
            if (mountainDiff > totalCount * nearEnough)
            {
                score -= mountainDiff;
            }

            // Adjust score depending on how close to target sea count it is
            int targetSeaCount = (int)(totalCount * 0.4f);
            int seaDiff = Math.Abs(targetSeaCount - seaCount);
            if (seaDiff > totalCount * nearEnough)
            {
                score -= seaDiff;
            }

            return score;
        }

        private int scoreLand(int x, int y)
        {
            int tempScore = 0;
            for (int xx = -1; xx < 2; xx++)
            {
                for (int yy = -1; yy < 2; yy++)
                {
                    // score up if connected to more land
                    if (isValidPoint(x + xx, y + yy))
                    {
                        if (pheno[x + xx, y + yy] == 1)
                        {
                            tempScore++;
                        }
                    }

                    // score down if there is too much land together (aiming for an Archipelago feel)
                    if (isValidPoint(x + xx * 20, y + yy * 20))
                    {
                        if (pheno[x + xx * 20, y + yy * 20] == 1)
                        {
                            tempScore--;
                        }
                    }
                }
            }

            return tempScore;
        }

        private int scoreMountains(int x, int y)
        {
            int tempScore = 0;
            int numDirectionsMountains = 0;
            for (int xx = -1; xx < 2; xx++)
            {
                for (int yy = -1; yy < 2; yy++)
                {
                    // score up if connected to more land
                    if (isValidPoint(x + xx, y + yy))
                        {
                        if (pheno[x + xx, y + yy] == 2)
                        {
                            numDirectionsMountains++;
                            // discorage more than 4 directions for mountains
                            if (numDirectionsMountains > 4)
                            {
                                tempScore--;
                            }
                            else
                            {
                                tempScore++;
                            }
                        }
                    }

                    // score down if there is too many mountains together
                    if (isValidPoint(x + xx * 5, y + yy * 5))
                    {
                        if (pheno[x + xx * 5, y + yy * 5] == 2)
                        {
                            tempScore--;
                        }
                    }
                }
            }

            return tempScore;
        }

        private int scoreSea(int x, int y)
        {
            int tempScore = 0;
            Object thisLock = new Object();

            for (int xx = -1; xx < 2; xx++)
            {
                for (int yy = -1; yy < 2; yy++)
                {
                    // score up if connected to more water
                    if (isValidPoint(x + xx, y + yy))
                    {
                        if (pheno[x + xx, y + yy] == 0)
                        {
                            tempScore++;
                        }
                    }

                    // score down if there is too much water together (we want rivers)
                    if (isValidPoint(x + xx*10, y + yy*10))
                    {
                        if (pheno[x + xx * 10, y + yy * 10] == 0)
                        {
                            tempScore--;
                        }
                    }

                    // score up if that water is near the edge
                    int margin = Params.dimX / 10;
                    if (x < margin || y < margin || x > Params.dimX - margin || y > Params.dimY - margin)
                    {
                        tempScore+=2;
                    }
                }
            }
            return tempScore;
        }

        private bool isValidPoint(int x, int y)
        {
            return x >= 0 && x < Params.dimX && y >= 0 && y < Params.dimY;
        }

        /// <summary>
        /// Display the map in a picturebox
        /// </summary>
        public void show(PictureBox pb)
        {
            System.Drawing.SolidBrush myBrush;
            if (bitm == null)
            {
                bitm = new Bitmap(Params.dimX, Params.dimY);
                myBrush = new System.Drawing.SolidBrush(G.ca[0]);
                Graphics gra = Graphics.FromImage(bitm);

                gra.FillRectangle(myBrush,0,0, Params.dimX, Params.dimY); //this is your code for drawing rectangles
                
                for (int x=0; x< Params.dimX; x++)
                {
                    for (int y = 0; y < Params.dimY; y++)
                    {
                        if (pheno[x,y] > 0)
                        {
                            bitm.SetPixel(x, y, G.ca[pheno[x,y]]);
                        }
                    }
                }
            }
            pb.Image = bitm;
        }
    }
}
