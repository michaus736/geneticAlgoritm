using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Text;


namespace GeneticAlgorithm{
    class Algorithm
    {
        int[] arr;
        const int populationSize = 100;
        const int innerLoops = 100;
        const int outerLoops = 5;
        const double mutationProbability = 0.008;
        const double crossoverProbability = 0.07;

        List<Genome> population;
        int sum;
        int prod;
        class Genome
        {
            //string genome;
            public string genome { get; set; }
            //double fitness;
            public double fitness { get; set; }
            // override object.Equals
            public override bool Equals(object obj)
            {

                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }

                Genome comp= (Genome)obj;
                return comp.genome == genome && comp.fitness == fitness;
            }

            // override object.GetHashCode
            public override int GetHashCode()
            {
                return (int)(fitness * 100);
            }
            // override object.ToString
            public override string ToString()
            {
                return String.Format("{0}, fitness: {1}", genome, fitness);
            }

            public static bool operator==(Genome a, string genomeName)
            {
                return a.genome==genomeName;
            }
            public static bool operator !=(Genome a, string genomeName)
            {

                return a.genome != genomeName;
            }
            public Genome(string genome_,double fitness_){
                genome = genome_;
                fitness = fitness_;
            }
        }
        public Algorithm(int[] arr_, int sum_,int prod_)
        {
            arr = arr_;
            sum = sum_;
            prod = prod_;
            run();
        }
        
        void generate()
        {
            population = new List<Genome>(populationSize);
            //creating genomes
            Random random = new Random();
            for(int i = 0; i < populationSize; i++)
            {
                string genome = "";
                do
                {
                    genome= generateGenomeName();
                    
                } while (population.Where(x => x == genome).Any());

                population.Add(new Genome(genome, fitness(genome)));

            }
        }

        string generateGenomeName()
        {
            Random random = new Random();
            string genome = "";
            for (int j = 0; j < arr.Length; j++)
            {
                double rand = random.NextDouble();
                genome += (rand > 0.5) ? "1" : "0";
            }

            return genome;
        }

        double fitness(string genome)
        {
            int sumFitness = 0, prodFitness = 1;
            for(int i = 0; i < genome.Length; i++)
            {
                if (genome[i] == '1') prodFitness *= arr[i];
                else sumFitness += arr[i];
            }
            double fitness = Math.Sqrt(Math.Pow(sum - sumFitness, 2) + Math.Pow(prod - prodFitness, 2));
            double normalize = 1 / (fitness + 1);
            return normalize;
        }
        
        void run()
        {
            generate();
            Random random = new Random();
            bool resultFlag = false;
            for(int loop = 0; loop < outerLoops; loop++)
            {
                Console.WriteLine((loop + 1) + " population");
                for(int i = 0; i < innerLoops; i++)
                {
                    Console.WriteLine((i + 1) + " genetation");
                    List<Genome> newpopulation = new List<Genome>(populationSize);
                    while (population.Count != 0)
                    {
                        List<Genome> pair=select();

                        population.Remove(pair[0]);
                        population.Remove(pair[1]);


                        double crossoverProbability = random.NextDouble();
                        double mutation1Probability = random.NextDouble();
                        double mutation2Probability = random.NextDouble();
                        if (crossoverProbability <= Algorithm.crossoverProbability) pair = crossover(pair);
                        if (mutation1Probability <= Algorithm.mutationProbability) pair[0] = mutation(pair[0]);
                        if (mutation2Probability <= Algorithm.mutationProbability) pair[1] = mutation(pair[1]);


                        newpopulation.AddRange(pair);



                    }
                    population = newpopulation;
                    if (population.Where(x => x.fitness == 1).Any()) {
                        resultFlag = true;
                        break;
                    }
                    Console.WriteLine("max fitness: " + population.Select(x => x.fitness).Max());
                }
                if (resultFlag) break;
            }
            var result = population.Where(x => x.fitness == 1).ToList();
            if (result.Count > 0)
            {
                Console.WriteLine("resultat");
                foreach (var genome in result)
                {
                    Console.WriteLine(genome.ToString());
                    List<int> prodList=new List<int>();
                    List<int> sumList=new List<int>();

                    for(int i = 0; i < arr.Length; i++)
                    {
                        if (genome.genome[i] == '1') prodList.Add(arr[i]);
                        else sumList.Add(arr[i]);
                    }
                    Console.WriteLine(string.Join<int>(" + ", sumList) + " = " + sum);
                    Console.WriteLine(string.Join<int>(" * ", prodList) + " = " + prod);
                }
            }
            else
            {
                Console.WriteLine("cannot find solution");
            }

        }


        List<Genome> select()
        {
            if (population.Count != 2)
            {
                List<double> probabilitySteps = new List<double>(populationSize);
                Random random = new Random();
                double rand;
                double fitnessSum = population.Select(x => x.fitness).Sum();
                probabilitySteps.Add(population[0].fitness / fitnessSum);
                for(int i = 1; i < population.Count; i++)
                {
                    probabilitySteps.Add(population[i].fitness / fitnessSum + probabilitySteps[i - 1]);
                }
                int index1 = -1, index2 = -1;
                rand = random.NextDouble();

                for(int i = 0; i < probabilitySteps.Count; i++)
                {
                    if (rand < probabilitySteps[i])
                    {
                        index1 = i;
                        break;
                    }
                }

                do
                {
                    rand = random.NextDouble();
                    for (int i = 0; i < probabilitySteps.Count; i++)
                    {
                        if (rand < probabilitySteps[i])
                        {
                            index2 = i;
                            break;
                        }
                    }
                } while (index1 == index2);




                return new List<Genome>() { population[index1], population[index2] };
            }
            else
            {
                return new List<Genome>() {population[0], population[1] };
            }
        }


        List<Genome> crossover(List<Genome> pair)
        {
            if (pair.Count != 2) throw new ArgumentException("isn't two genomes in crossover");
            Random random = new Random();
            int index = random.Next(1, arr.Length);
            string a1 = pair[0].genome.Substring(0, index);
            string b1 = pair[1].genome.Substring(0, index);
            string a2 = pair[0].genome.Substring(index);
            string b2 = pair[1].genome.Substring(index);
            string genome1 = a1 + b2;
            string genome2 = b1 + a2;
            pair[0] = new Genome(genome1, fitness(genome1));
            pair[1] = new Genome(genome2, fitness(genome2));
            return pair;
        }
        Genome mutation(Genome genome)
        {
            Random random = new Random();
            int index = random.Next(0, arr.Length);
            StringBuilder stringBuilder = new StringBuilder(genome.genome);
            stringBuilder[index] = (genome.genome[index] == '0') ? '1' : '0';
            return new Genome(stringBuilder.ToString(), fitness(stringBuilder.ToString()));
        }

    }





    

}


namespace ConsoleApp1
{

    class Program
    {
        static void Main()
        {

            GeneticAlgorithm.Algorithm binear = new GeneticAlgorithm.Algorithm(new int[] { 0,0,0,0,1,15,14,12,6,27,17 }, 54, 1512);




        }

    }

}