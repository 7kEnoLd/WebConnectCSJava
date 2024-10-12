using Google.OrTools.LinearSolver;

class BusScheduleOptimization
{
    
    public static OutputBusSchedule OptimizationOrtools(BusSchedule busSchedule)
    {
        // Create the solver
        Solver solver = Solver.CreateSolver("CBC_MIXED_INTEGER_PROGRAMMING");

        int M = 100000;
        int[] carCount = new int[busSchedule.timeInterval.Length];
        for (int i = 0; i < busSchedule.timeInterval.Length; i++)
        {
            carCount[i] = (int)Math.Ceiling((double)busSchedule.totalTimeInterval / (double)busSchedule.timeInterval[i]);
        }

        // Decision variables x[i, j] (integer variables)
        Variable[,] x = new Variable[busSchedule.timeInterval.Length, carCount.Max()];
        for (int i = 0; i < busSchedule.timeInterval.Length; i++)
        {
            for (int j = 0; j < carCount[i]; j++)
            {
                x[i, j] = solver.MakeIntVar(0, busSchedule.totalTimeInterval, $"x{i}_{j}");
            }
        }

        // Decision variables y[i, j, k, l, m] (binary variables)
        Variable[,,,,] y = new Variable[busSchedule.timeInterval.Length, carCount.Max(), busSchedule.timeInterval.Length, carCount.Max(), busSchedule.timeRunning[0].Length];
        Objective objective = solver.Objective();
        for (int i = 0; i < busSchedule.timeInterval.Length; i++)
        {
            for (int j = 0; j < carCount[i]; j++)
            {
                for (int k = 0; k < busSchedule.timeInterval.Length; k++)
                {
                    for (int l = 0; l < carCount[k]; l++)
                    {
                        for (int m = 0; m < busSchedule.timeRunning[0].Length; m++)
                        {
                            y[i, j, k, l, m] = solver.MakeBoolVar($"y{i}_{j}_{k}_{l}_{m}");
                            if (i != k)
                            {
                                objective.SetCoefficient(y[i, j, k, l, m], 1);
                            }
                        }
                    }
                }
            }
        }

        // Constraints for y variables
        // Fixing constraint with sum of boolean variables
        for (int i = 0; i < busSchedule.timeInterval.Length; i++)
        {
            for (int k = 0; k < busSchedule.timeInterval.Length; k++)
            {
                for (int l = 0; l < carCount[k]; l++)
                {
                    for (int m = 0; m < busSchedule.timeRunning[0].Length; m++)
                    {
                        if (busSchedule.timeRunning[i][m] == busSchedule.timeLimit || busSchedule.timeRunning[k][m] == busSchedule.timeLimit)
                        {
                            for (int j = 0; j < carCount[i]; j++)
                            {
                                solver.Add(y[i, j, k, l, m] == 0);
                            }
                        }

                        Constraint constraint = solver.MakeConstraint(double.NegativeInfinity, 10, $"constraint1:{i} {k} {l} {m}");

                        for (int j = 0; j < carCount[i]; j++)
                        {
                            if (j == 0)
                            {
                                solver.Add(x[i, j] + busSchedule.timeRunning[i][m] - x[k, l] - busSchedule.timeRunning[k][m] - busSchedule.timeTransfer[m] >= -M * (1 - y[i, j, k, l, m]));
                                solver.Add(x[i, j] + busSchedule.timeRunning[i][m] - x[k, l] - busSchedule.timeRunning[k][m] - busSchedule.timeTransfer[m] <= busSchedule.timeRunning[i][m] - 1 + M * (1 - y[i, j, k, l, m]));
                            }
                            else
                            {
                                solver.Add(x[i, j] + busSchedule.timeRunning[i][m] - x[k, l] - busSchedule.timeRunning[k][m] - busSchedule.timeTransfer[m] >= -M * (1 - y[i, j, k, l, m]));
                                solver.Add(x[i, j] + busSchedule.timeRunning[i][m] - x[k, l] - busSchedule.timeRunning[k][m] - busSchedule.timeTransfer[m] <= busSchedule.timeInterval[i] - 1 + M * (1 - y[i, j, k, l, m]));
                            }

                            constraint.SetCoefficient(y[i, j, k, l, m], 1);
                        }
                    }
                }
            }
        }


        // Constraints for x variables (separation constraint)
        for (int i = 0; i < busSchedule.timeInterval.Length; i++)
        {
            for (int j = 0; j < carCount[i] - 1; j++)
            {
                solver.Add(x[i, j + 1] - x[i, j] == busSchedule.timeInterval[i]);
            }
        }

        // Define the objective function (maximization)
        objective.SetMaximization();

        // Solve the model
        Solver.ResultStatus resultStatus = solver.Solve();

        // output the result
        int[] firstCar = new int[busSchedule.timeInterval.Length];
        for (int i = 0; i < busSchedule.timeInterval.Length; i++)
        {
            firstCar[i] = (int)x[i, 0].SolutionValue();
        }
        OutputBusSchedule outputBusSchedule = new OutputBusSchedule((int)solver.Objective().Value(), firstCar);

        return outputBusSchedule;
    }
}