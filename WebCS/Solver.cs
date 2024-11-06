using Google.OrTools.Sat;

public class BusScheduleOptimization
{
    
    public static OutputBusSchedule OptimizationOrtools(BusSchedule busSchedule)
    {
        // Create the CP-SAT model
        CpModel model = new CpModel();

        int M = 100000;
        int[] carCount = new int[busSchedule.timeInterval.Length];
        for (int i = 0; i < busSchedule.timeInterval.Length; i++)
        {
            carCount[i] = (int)Math.Ceiling((double)busSchedule.totalTimeInterval / (double)busSchedule.timeInterval[i]);
        }

        // Decision variables x[i, j] (integer variables)
        IntVar[,] x = new IntVar[busSchedule.timeInterval.Length, carCount.Max()];
        for (int i = 0; i < busSchedule.timeInterval.Length; i++)
        {
            for (int j = 0; j < carCount[i]; j++)
            {
                if ((j + 1) * busSchedule.timeInterval[i] >= busSchedule.totalTimeInterval)
                {
                    x[i, j] = model.NewIntVar(j * busSchedule.timeInterval[i], busSchedule.totalTimeInterval, $"x{i}_{j}");
                }
                else
                {
                    x[i, j] = model.NewIntVar(j * busSchedule.timeInterval[i], (j + 1) * busSchedule.timeInterval[i], $"x{i}_{j}");
                }
            }
        }

        // Decision variables y[i, j, k, l, m] (binary variables)
        IntVar[,,,,] y = new IntVar[busSchedule.timeInterval.Length, carCount.Max(), busSchedule.timeInterval.Length, carCount.Max(), busSchedule.timeRunning[0].Length];
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
                            y[i, j, k, l, m] = model.NewBoolVar($"y{i}_{j}_{k}_{l}_{m}");
                        }
                    }
                }
            }
        }

        // Constraints for y variables
        for (int i = 0; i < busSchedule.timeInterval.Length; i++)
        {
            for (int k = 0; k < busSchedule.timeInterval.Length; k++)
            {
                for (int l = 0; l < carCount[k]; l++)
                {
                    for (int m = 0; m < busSchedule.timeRunning[0].Length; m++)
                    {
                        // situation 1: cannot transfer between two lines
                        if (busSchedule.timeRunning[i][m] == busSchedule.timeLimit || busSchedule.timeRunning[k][m] == busSchedule.timeLimit)
                        {
                            for (int j = 0; j < carCount[i]; j++)
                            {
                                model.Add(y[i, j, k, l, m] == 0);
                            }
                            continue;
                        }


                        for (int j = 0; j < carCount[i]; j++)
                        {
                            // situation 2: cannot transfer between two vehicles
                            if (i != k)
                            {
                                if (j == 0)
                                {
                                    if (!((busSchedule.timeRunning[k][m] + busSchedule.timeTransfer[m] + l * busSchedule.timeInterval[k] > (j + 1) * busSchedule.timeInterval[i] - 1 + busSchedule.timeRunning[i][m]) || (busSchedule.timeRunning[k][m] + busSchedule.timeTransfer[m] + (l + 1) * busSchedule.timeInterval[k] - 1 + busSchedule.timeRunning[i][m] - 1 < j * busSchedule.timeInterval[i] + busSchedule.timeRunning[i][m])))
                                    {
                                        model.Add(x[i, j] + busSchedule.timeRunning[i][m] - x[k, l] - busSchedule.timeRunning[k][m] - busSchedule.timeTransfer[m] >= -M * (1 - y[i, j, k, l, m]));
                                        model.Add(x[i, j] + busSchedule.timeRunning[i][m] - x[k, l] - busSchedule.timeRunning[k][m] - busSchedule.timeTransfer[m] <= busSchedule.timeRunning[i][m] - 1 + M * (1 - y[i, j, k, l, m]));
                                    }
                                    else
                                    {
                                        model.Add(y[i, j, k, l, m] == 0);
                                    }
                                }
                                else
                                {
                                    if (!((busSchedule.timeRunning[k][m] + busSchedule.timeTransfer[m] + l * busSchedule.timeInterval[k] > (j + 1) * busSchedule.timeInterval[i] - 1 + busSchedule.timeRunning[i][m]) || (busSchedule.timeRunning[k][m] + busSchedule.timeTransfer[m] + (l + 1) * busSchedule.timeInterval[k] - 1 + busSchedule.timeInterval[i] - 1 < j * busSchedule.timeInterval[i] + busSchedule.timeRunning[i][m])))
                                    {
                                        model.Add(x[i, j] + busSchedule.timeRunning[i][m] - x[k, l] - busSchedule.timeRunning[k][m] - busSchedule.timeTransfer[m] >= -M * (1 - y[i, j, k, l, m]));
                                        model.Add(x[i, j] + busSchedule.timeRunning[i][m] - x[k, l] - busSchedule.timeRunning[k][m] - busSchedule.timeTransfer[m] <= busSchedule.timeInterval[i] - 1 + M * (1 - y[i, j, k, l, m]));
                                    }
                                    else
                                    {
                                        model.Add(y[i, j, k, l, m] == 0);
                                    }
                                }
                            }
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
                model.Add(x[i, j + 1] - x[i, j] == busSchedule.timeInterval[i]);
            }
        }

        // Define the objective function (maximization)
        Google.OrTools.Sat.LinearExpr objective = Google.OrTools.Sat.LinearExpr.Sum(
            from i in Enumerable.Range(0, busSchedule.timeInterval.Length)
            from j in Enumerable.Range(0, carCount[i])
            from k in Enumerable.Range(0, busSchedule.timeInterval.Length)
            from l in Enumerable.Range(0, carCount[k])
            from m in Enumerable.Range(0, busSchedule.timeRunning[0].Length)
            where i != k
            select y[i, j, k, l, m]
        );
        model.Maximize(objective);

        // Solve the model
        CpSolver solver = new()
        {
            // 设置求解时间上限
            StringParameters = $"max_time_in_seconds:{busSchedule.solverTimeLimit}"
        };
        CpSolverStatus resultStatus = solver.Solve(model);

        // Output the result
        if (resultStatus == CpSolverStatus.Optimal || resultStatus == CpSolverStatus.Feasible)
        {
            Console.WriteLine("最优解:");
            Console.WriteLine($"最大化目标函数值 = {solver.ObjectiveValue}");
        }
        else
        {
            Console.WriteLine("无法找到最优解。");
        }

        // output the result
        int[] firstCar = new int[busSchedule.timeInterval.Length];
        for (int i = 0; i < busSchedule.timeInterval.Length; i++)
        {
            firstCar[i] = (int)solver.Value(x[i, 0]);
        }
        OutputBusSchedule outputBusSchedule = new((int)solver.ObjectiveValue, firstCar);

        return outputBusSchedule;
    }
}