using System.Diagnostics;
using Google.OrTools.Sat;

public class BusScheduleOptimization
{
    public static OutputBusSchedule OptimizationOrtools(BusSchedule busSchedule)
    {
        switch (busSchedule.ModelFlag)
        {
            case 1:
                return Optimization_CP_SAT_Model1(busSchedule);
            case 2:
                return Optimization_CP_SAT_Model2(busSchedule);
            case 3:
                return Optimization_CP_SAT_Model3(busSchedule);
            case 4:
                return Optimization_CP_SAT_Model4(busSchedule);
            default:
                return null;
        }
    }

    private static OutputBusSchedule Optimization_CP_SAT_Model1(BusSchedule busSchedule)
    {
        // Create the CP-SAT model
        CpModel model = new CpModel();

        int M = 100000;
        int[] carCount = new int[busSchedule.TimeInterval.Length];
        for (int i = 0; i < busSchedule.TimeInterval.Length; i++)
        {
            carCount[i] = (int)Math.Ceiling((double)busSchedule.TotalTimeInterval / (double)busSchedule.TimeInterval[i]);
        }

        // Decision variables x[i, j] (integer variables)
        IntVar[,] x = new IntVar[busSchedule.TimeInterval.Length, carCount.Max()];
        for (int i = 0; i < busSchedule.TimeInterval.Length; i++)
        {
            for (int j = 0; j < carCount[i]; j++)
            {
                if (j == 0 && carCount[i] != 1 && (j + 1) * busSchedule.TimeInterval[i] - busSchedule.LastCar[i] > 0)
                {
                    // The first car of each line must start at time 0 and end at time with last car
                    x[i, j] = model.NewIntVar(j * busSchedule.TimeInterval[i], (j + 1) * busSchedule.TimeInterval[i] - busSchedule.LastCar[i], $"x{i}_{j}");
                }
                else if ((j + 1) * busSchedule.TimeInterval[i] >= busSchedule.TotalTimeInterval)
                {
                    x[i, j] = model.NewIntVar(j * busSchedule.TimeInterval[i], busSchedule.TotalTimeInterval, $"x{i}_{j}");
                }
                else
                {
                    x[i, j] = model.NewIntVar(j * busSchedule.TimeInterval[i], (j + 1) * busSchedule.TimeInterval[i], $"x{i}_{j}");
                }
            }
        }

        // Decision variables y[i, j, k, l, m] (binary variables)
        IntVar[,,,,] y = new IntVar[busSchedule.TimeInterval.Length, carCount.Max(), busSchedule.TimeInterval.Length, carCount.Max(), busSchedule.TimeRunning[0].Length];
        for (int i = 0; i < busSchedule.TimeInterval.Length; i++)
        {
            for (int j = 0; j < carCount[i]; j++)
            {
                for (int k = 0; k < busSchedule.TimeInterval.Length; k++)
                {
                    for (int l = 0; l < carCount[k]; l++)
                    {
                        for (int m = 0; m < busSchedule.TimeRunning[0].Length; m++)
                        {
                            y[i, j, k, l, m] = model.NewBoolVar($"y{i}_{j}_{k}_{l}_{m}");
                        }
                    }
                }
            }
        }

        // Constraints for y variables
        for (int i = 0; i < busSchedule.TimeInterval.Length; i++)
        {
            for (int k = 0; k < busSchedule.TimeInterval.Length; k++)
            {
                for (int l = 0; l < carCount[k]; l++)
                {
                    for (int m = 0; m < busSchedule.TimeRunning[0].Length; m++)
                    {
                        // situation 1: cannot transfer between two lines
                        if (busSchedule.TimeRunning[i][m] == busSchedule.TimeLimit || busSchedule.TimeRunning[k][m] == busSchedule.TimeLimit)
                        {
                            for (int j = 0; j < carCount[i]; j++)
                            {
                                model.Add(y[i, j, k, l, m] == 0);
                            }
                            continue;
                        }
                        model.AddLinearConstraint(Google.OrTools.Sat.LinearExpr.Sum(Enumerable.Range(0, carCount[i]).Select(j => y[i, j, k, l, m])), 0, 1);
                        for (int j = 0; j < carCount[i]; j++)
                        {
                            // situation 2: cannot transfer between two vehicles
                            if (i != k)
                            {
                                if (j == 0)
                                {
                                    if (!((busSchedule.TimeRunning[k][m] + busSchedule.TimeTransfer[m] + l * busSchedule.TimeInterval[k] > (j + 1) * busSchedule.TimeInterval[i] - 1 + busSchedule.TimeRunning[i][m]) || (busSchedule.TimeRunning[k][m] + busSchedule.TimeTransfer[m] + (l + 1) * busSchedule.TimeInterval[k] - 1 + busSchedule.TimeRunning[i][m] - 1 < j * busSchedule.TimeInterval[i] + busSchedule.TimeRunning[i][m])))
                                    {
                                        model.Add(x[i, j] + busSchedule.TimeRunning[i][m] - x[k, l] - busSchedule.TimeRunning[k][m] - busSchedule.TimeTransfer[m] >= -M * (1 - y[i, j, k, l, m]));
                                        model.Add(x[i, j] + busSchedule.TimeRunning[i][m] - x[k, l] - busSchedule.TimeRunning[k][m] - busSchedule.TimeTransfer[m] <= busSchedule.TimeRunning[i][m] + M * (1 - y[i, j, k, l, m]));
                                    }
                                    else
                                    {
                                        model.Add(y[i, j, k, l, m] == 0);
                                    }
                                }
                                else
                                {
                                    if (!((busSchedule.TimeRunning[k][m] + busSchedule.TimeTransfer[m] + l * busSchedule.TimeInterval[k] > (j + 1) * busSchedule.TimeInterval[i] - 1 + busSchedule.TimeRunning[i][m]) || (busSchedule.TimeRunning[k][m] + busSchedule.TimeTransfer[m] + (l + 1) * busSchedule.TimeInterval[k] - 1 + busSchedule.TimeInterval[i] - 1 < j * busSchedule.TimeInterval[i] + busSchedule.TimeRunning[i][m])))
                                    {
                                        model.Add(x[i, j] + busSchedule.TimeRunning[i][m] - x[k, l] - busSchedule.TimeRunning[k][m] - busSchedule.TimeTransfer[m] >= -M * (1 - y[i, j, k, l, m]));
                                        model.Add(x[i, j] + busSchedule.TimeRunning[i][m] - x[k, l] - busSchedule.TimeRunning[k][m] - busSchedule.TimeTransfer[m] <= busSchedule.TimeInterval[i] - 1 + M * (1 - y[i, j, k, l, m]));
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
        for (int i = 0; i < busSchedule.TimeInterval.Length; i++)
        {
            for (int j = 0; j < carCount[i] - 1; j++)
            {
                model.Add(x[i, j + 1] - x[i, j] == busSchedule.TimeInterval[i]);
            }
        }

        // Define the objective function (maximization)
        Google.OrTools.Sat.LinearExpr objective = Google.OrTools.Sat.LinearExpr.Sum(
            from i in Enumerable.Range(0, busSchedule.TimeInterval.Length)
            from j in Enumerable.Range(0, carCount[i])
            from k in Enumerable.Range(0, busSchedule.TimeInterval.Length)
            from l in Enumerable.Range(0, carCount[k])
            from m in Enumerable.Range(0, busSchedule.TimeRunning[0].Length)
            where i != k
            select y[i, j, k, l, m]
        );
        model.Maximize(objective);

        // Solve the model
        CpSolver solver = new CpSolver();
        // Set a time limit for the solver
        solver.StringParameters = $"max_time_in_seconds:{busSchedule.SolverTimeLimit}";
        CpSolverStatus resultStatus = solver.Solve(model);

        // Output the result
        int modelStatus;
        if (resultStatus == CpSolverStatus.Optimal)
        {
            modelStatus = 0;
        }
        else if (resultStatus == CpSolverStatus.Infeasible)
        {
            modelStatus = 2;
        }
        else
        {
            modelStatus = 1;
        }

        // output the result
        int[] firstCar = new int[busSchedule.TimeInterval.Length];
        for (int i = 0; i < busSchedule.TimeInterval.Length; i++)
        {
            firstCar[i] = (int)solver.Value(x[i, 0]);
        }
        OutputBusSchedule outputBusSchedule = new(modelStatus, solver.ObjectiveValue, firstCar);

        return outputBusSchedule;
    }

    private static OutputBusSchedule Optimization_CP_SAT_Model2(BusSchedule busSchedule)
    {
        double[,,] transferPassengerFlow = new double[busSchedule.TimeInterval.Length, busSchedule.TimeInterval.Length, busSchedule.TimeRunning[0].Length];
        // Parse integer
        for (int i = 0; i < busSchedule.TimeInterval.Length; i++)
        {
            for (int j = 0; j < busSchedule.TimeInterval.Length; j++)
            {
                for (int k = 0; k < busSchedule.TimeRunning[0].Length; k++)
                {
                    transferPassengerFlow[i, j, k] = busSchedule.TransferPassengerFlow[i][j][k];
                }
            }
        }

        // Create the CP-SAT model
        CpModel model = new CpModel();

        int M = 100000;
        int[] carCount = new int[busSchedule.TimeInterval.Length];
        for (int i = 0; i < busSchedule.TimeInterval.Length; i++)
        {
            carCount[i] = (int)Math.Ceiling((double)busSchedule.TotalTimeInterval / (double)busSchedule.TimeInterval[i]); //向上取整
        }

        // Decision variables x[i, j] (integer variables)
        IntVar[,] x = new IntVar[busSchedule.TimeInterval.Length, carCount.Max()];
        for (int i = 0; i < busSchedule.TimeInterval.Length; i++)
        {
            for (int j = 0; j < carCount[i]; j++)
            {
                if (j == 0 && carCount[i] != 1 && (j + 1) * busSchedule.TimeInterval[i] - busSchedule.LastCar[i] > 0)
                {
                    // The first car of each line must start at time 0 and end at time with last car
                    x[i, j] = model.NewIntVar(j * busSchedule.TimeInterval[i], (j + 1) * busSchedule.TimeInterval[i] - busSchedule.LastCar[i], $"x{i}_{j}");
                }
                else if ((j + 1) * busSchedule.TimeInterval[i] >= busSchedule.TotalTimeInterval)
                {
                    x[i, j] = model.NewIntVar(j * busSchedule.TimeInterval[i], busSchedule.TotalTimeInterval, $"x{i}_{j}");
                }
                else
                {
                    x[i, j] = model.NewIntVar(j * busSchedule.TimeInterval[i], (j + 1) * busSchedule.TimeInterval[i], $"x{i}_{j}");
                }
            }
        }

        // Decision variables y[i, j, k, l, m] (binary variables)
        IntVar[,,,,] y = new IntVar[busSchedule.TimeInterval.Length, carCount.Max(), busSchedule.TimeInterval.Length, carCount.Max(), busSchedule.TimeRunning[0].Length];
        for (int i = 0; i < busSchedule.TimeInterval.Length; i++)
        {
            for (int j = 0; j < carCount[i]; j++)
            {
                for (int k = 0; k < busSchedule.TimeInterval.Length; k++)
                {
                    for (int l = 0; l < carCount[k]; l++)
                    {
                        for (int m = 0; m < busSchedule.TimeRunning[0].Length; m++)
                        {
                            y[i, j, k, l, m] = model.NewBoolVar($"y{i}_{j}_{k}_{l}_{m}");
                        }
                    }
                }
            }
        }

        // Constraints for y variables
        for (int i = 0; i < busSchedule.TimeInterval.Length; i++)
        {
            for (int k = 0; k < busSchedule.TimeInterval.Length; k++)
            {
                for (int l = 0; l < carCount[k]; l++)
                {
                    for (int m = 0; m < busSchedule.TimeRunning[0].Length; m++)
                    {
                        // situation 1: cannot transfer between two lines
                        if (busSchedule.TimeRunning[i][m] == busSchedule.TimeLimit || busSchedule.TimeRunning[k][m] == busSchedule.TimeLimit)
                        {
                            for (int j = 0; j < carCount[i]; j++)
                            {
                                model.Add(y[i, j, k, l, m] == 0);
                            }
                            continue;
                        }
                        // model.AddLinearConstraint(Google.OrTools.Sat.LinearExpr.Sum(Enumerable.Range(0, carCount[i]).Select(j => y[i, j, k, l, m])), 0, 1);
                        for (int j = 0; j < carCount[i]; j++)
                        {
                            // situation 2: cannot transfer between two vehicles
                            if (i != k)
                            {
                                if (j == 0)
                                {
                                    if (!((busSchedule.TimeRunning[k][m] + busSchedule.TimeTransfer[m] + l * busSchedule.TimeInterval[k] > (j + 1) * busSchedule.TimeInterval[i] - 1 + busSchedule.TimeRunning[i][m]) || (busSchedule.TimeRunning[k][m] + busSchedule.TimeTransfer[m] + (l + 1) * busSchedule.TimeInterval[k] - 1 + busSchedule.TimeRunning[i][m] - 1 < j * busSchedule.TimeInterval[i] + busSchedule.TimeRunning[i][m])))
                                    {
                                        model.Add(x[i, j] + busSchedule.TimeRunning[i][m] - x[k, l] - busSchedule.TimeRunning[k][m] - busSchedule.TimeTransfer[m] >= -M * (1 - y[i, j, k, l, m]));
                                        model.Add(x[i, j] + busSchedule.TimeRunning[i][m] - x[k, l] - busSchedule.TimeRunning[k][m] - busSchedule.TimeTransfer[m] <= busSchedule.TimeRunning[i][m] + M * (1 - y[i, j, k, l, m]));
                                    }
                                    else
                                    {
                                        model.Add(y[i, j, k, l, m] == 0);
                                    }
                                }
                                else
                                {
                                    if (!((busSchedule.TimeRunning[k][m] + busSchedule.TimeTransfer[m] + l * busSchedule.TimeInterval[k] > (j + 1) * busSchedule.TimeInterval[i] - 1 + busSchedule.TimeRunning[i][m]) || (busSchedule.TimeRunning[k][m] + busSchedule.TimeTransfer[m] + (l + 1) * busSchedule.TimeInterval[k] - 1 + busSchedule.TimeInterval[i] - 1 < j * busSchedule.TimeInterval[i] + busSchedule.TimeRunning[i][m])))
                                    {
                                        model.Add(x[i, j] + busSchedule.TimeRunning[i][m] - x[k, l] - busSchedule.TimeRunning[k][m] - busSchedule.TimeTransfer[m] >= -M * (1 - y[i, j, k, l, m]));
                                        model.Add(x[i, j] + busSchedule.TimeRunning[i][m] - x[k, l] - busSchedule.TimeRunning[k][m] - busSchedule.TimeTransfer[m] <= busSchedule.TimeInterval[i] - 1 + M * (1 - y[i, j, k, l, m]));
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
        for (int i = 0; i < busSchedule.TimeInterval.Length; i++)
        {
            for (int j = 0; j < carCount[i] - 1; j++)
            {
                model.Add(x[i, j + 1] - x[i, j] == busSchedule.TimeInterval[i]);
            }
        }

        // Define the objective function (maximization)
        int scaleFactor = 10;
        var objective = Google.OrTools.Sat.LinearExpr.Sum(
            from i in Enumerable.Range(0, busSchedule.TimeInterval.Length)
            from k in Enumerable.Range(0, busSchedule.TimeInterval.Length)
            from m in Enumerable.Range(0, busSchedule.TimeRunning[0].Length)
            let sumJl = Google.OrTools.Sat.LinearExpr.Sum(
                from j in Enumerable.Range(0, carCount[i])
                from l in Enumerable.Range(0, carCount[k])
                where i != k
                select y[i, j, k, l, m]
            )
            select sumJl * (int)(transferPassengerFlow[k, i, m] * scaleFactor)
        );

        model.Maximize(objective);

        // Solve the model
        CpSolver solver = new CpSolver();
        // Set the solver time limit
        solver.StringParameters = $"max_time_in_seconds:{busSchedule.SolverTimeLimit}";
        CpSolverStatus resultStatus = solver.Solve(model);

        // Output the result
        int modelStatus;
        if (resultStatus == CpSolverStatus.Optimal)
        {
            modelStatus = 0;
        }
        else if (resultStatus == CpSolverStatus.Infeasible)
        {
            modelStatus = 2;
        }
        else
        {
            modelStatus = 1;
        }

        // output the result
        int[] firstCar = new int[busSchedule.TimeInterval.Length];
        for (int i = 0; i < busSchedule.TimeInterval.Length; i++)
        {
            firstCar[i] = (int)solver.Value(x[i, 0]);
        }
        OutputBusSchedule outputBusSchedule = new(modelStatus, solver.ObjectiveValue, firstCar);

        return outputBusSchedule;
    }

    private static OutputBusSchedule Optimization_CP_SAT_Model3(BusSchedule busSchedule)
    {
        double[,,] transferPassengerFlow = new double[busSchedule.TimeInterval.Length, busSchedule.TimeInterval.Length, busSchedule.TimeRunning[0].Length];
        // Parse integer
        for (int i = 0; i < busSchedule.TimeInterval.Length; i++)
        {
            for (int j = 0; j < busSchedule.TimeInterval.Length; j++)
            {
                for (int k = 0; k < busSchedule.TimeRunning[0].Length; k++)
                {
                    transferPassengerFlow[i, j, k] = 10 * busSchedule.TransferPassengerFlow[i][j][k];
                }
            }
        }

        // Create the CP-SAT model
        CpModel model = new CpModel();

        int M = 100000;
        int[] carCount = new int[busSchedule.TimeInterval.Length];
        for (int i = 0; i < busSchedule.TimeInterval.Length; i++)
        {
            carCount[i] = (int)Math.Ceiling((double)busSchedule.TotalTimeInterval / (double)busSchedule.TimeInterval[i]); //向上取整
        }

        // Decision variables x[i, j] (integer variables)
        IntVar[,] x = new IntVar[busSchedule.TimeInterval.Length, carCount.Max()];
        for (int i = 0; i < busSchedule.TimeInterval.Length; i++)
        {
            for (int j = 0; j < carCount[i]; j++)
            {
                if (j == 0 && carCount[i] != 1 && (j + 1) * busSchedule.TimeInterval[i] - busSchedule.LastCar[i] > 0)
                {
                    // The first car of each line must start at time 0 and end at time with last car
                    x[i, j] = model.NewIntVar(j * busSchedule.TimeInterval[i], (j + 1) * busSchedule.TimeInterval[i] - busSchedule.LastCar[i], $"x{i}_{j}");
                }
                else if ((j + 1) * busSchedule.TimeInterval[i] >= busSchedule.TotalTimeInterval)
                {
                    x[i, j] = model.NewIntVar(j * busSchedule.TimeInterval[i], busSchedule.TotalTimeInterval, $"x{i}_{j}");
                }
                else
                {
                    x[i, j] = model.NewIntVar(j * busSchedule.TimeInterval[i], (j + 1) * busSchedule.TimeInterval[i], $"x{i}_{j}");
                }
            }
        }

        // Decision variables y[i, j, k, l, m] (binary variables)
        IntVar[,,,,] y = new IntVar[busSchedule.TimeInterval.Length, carCount.Max(), busSchedule.TimeInterval.Length, carCount.Max(), busSchedule.TimeRunning[0].Length];
        IntVar[,,,,] w = new IntVar[busSchedule.TimeInterval.Length, carCount.Max(), busSchedule.TimeInterval.Length, carCount.Max(), busSchedule.TimeRunning[0].Length];
        IntVar[,,,,] u = new IntVar[busSchedule.TimeInterval.Length, carCount.Max(), busSchedule.TimeInterval.Length, carCount.Max(), busSchedule.TimeRunning[0].Length];
        for (int i = 0; i < busSchedule.TimeInterval.Length; i++)
        {
            for (int j = 0; j < carCount[i]; j++)
            {
                for (int k = 0; k < busSchedule.TimeInterval.Length; k++)
                {
                    for (int l = 0; l < carCount[k]; l++)
                    {
                        for (int m = 0; m < busSchedule.TimeRunning[0].Length; m++)
                        {
                            if (i != k)
                            {
                                y[i, j, k, l, m] = model.NewBoolVar($"y{i}_{j}_{k}_{l}_{m}");
                                w[i, j, k, l, m] = model.NewIntVar(0, busSchedule.TotalTimeInterval, $"w{i}_{j}_{k}_{l}_{m}");
                                u[i, j, k, l, m] = model.NewIntVar(0, busSchedule.TotalTimeInterval, $"u{i}_{j}_{k}_{l}_{m}");
                                model.Add(u[i, j, k, l, m] == w[i, j, k, l, m]).OnlyEnforceIf((ILiteral)y[i, j, k, l, m]);
                                model.Add(u[i, j, k, l, m] == 0).OnlyEnforceIf(((ILiteral)y[i, j, k, l, m]).Not());
                            }
                            else
                            {
                                y[i, j, k, l, m] = model.NewBoolVar($"y{i}_{j}_{k}_{l}_{m}");
                                w[i, j, k, l, m] = model.NewIntVar(0, 0, $"w{i}_{j}_{k}_{l}_{m}");
                                u[i, j, k, l, m] = model.NewIntVar(0, 0, $"u{i}_{j}_{k}_{l}_{m}");
                                model.Add(y[i, j, k, l, m] == 0);
                            }
                        }
                    }
                }
            }
        }

        // Constraints for y variables
        for (int i = 0; i < busSchedule.TimeInterval.Length; i++)
        {
            for (int k = 0; k < busSchedule.TimeInterval.Length; k++)
            {
                for (int l = 0; l < carCount[k]; l++)
                {
                    for (int m = 0; m < busSchedule.TimeRunning[0].Length; m++)
                    {
                        // situation 1: cannot transfer between two lines
                        if (busSchedule.TimeRunning[i][m] == busSchedule.TimeLimit || busSchedule.TimeRunning[k][m] == busSchedule.TimeLimit)
                        {
                            for (int j = 0; j < carCount[i]; j++)
                            {
                                model.Add(y[i, j, k, l, m] == 0);
                            }
                            continue;
                        }
                        model.AddLinearConstraint(Google.OrTools.Sat.LinearExpr.Sum(Enumerable.Range(0, carCount[i]).Select(j => y[i, j, k, l, m])), 0, 1);
                        for (int j = 0; j < carCount[i]; j++)
                        {
                            // situation 2: cannot transfer between two vehicles
                            if (i != k)
                            {
                                if (j == 0)
                                {
                                    if (!((busSchedule.TimeRunning[k][m] + busSchedule.TimeTransfer[m] + l * busSchedule.TimeInterval[k] > (j + 1) * busSchedule.TimeInterval[i] - 1 + busSchedule.TimeRunning[i][m]) || (busSchedule.TimeRunning[k][m] + busSchedule.TimeTransfer[m] + (l + 1) * busSchedule.TimeInterval[k] - 1 + busSchedule.TimeRunning[i][m] - 1 < j * busSchedule.TimeInterval[i] + busSchedule.TimeRunning[i][m])))
                                    {
                                        model.Add(x[i, j] + busSchedule.TimeRunning[i][m] - x[k, l] - busSchedule.TimeRunning[k][m] - busSchedule.TimeTransfer[m] >= -M * (1 - y[i, j, k, l, m]));
                                        model.Add(x[i, j] + busSchedule.TimeRunning[i][m] - x[k, l] - busSchedule.TimeRunning[k][m] - busSchedule.TimeTransfer[m] <= busSchedule.TimeRunning[i][m] + M * (1 - y[i, j, k, l, m]));

                                        model.Add(w[i, j, k, l, m] <= x[i, j] + busSchedule.TimeRunning[i][m] - x[k, l] - busSchedule.TimeRunning[k][m] - busSchedule.TimeTransfer[m] + M * (1 - y[i, j, k, l, m]));
                                        model.Add(w[i, j, k, l, m] >= x[i, j] + busSchedule.TimeRunning[i][m] - x[k, l] - busSchedule.TimeRunning[k][m] - busSchedule.TimeTransfer[m] - M * (1 - y[i, j, k, l, m]));
                                    }
                                    else
                                    {
                                        model.Add(y[i, j, k, l, m] == 0);
                                    }
                                }
                                else
                                {
                                    if (!((busSchedule.TimeRunning[k][m] + busSchedule.TimeTransfer[m] + l * busSchedule.TimeInterval[k] > (j + 1) * busSchedule.TimeInterval[i] - 1 + busSchedule.TimeRunning[i][m]) || (busSchedule.TimeRunning[k][m] + busSchedule.TimeTransfer[m] + (l + 1) * busSchedule.TimeInterval[k] - 1 + busSchedule.TimeInterval[i] - 1 < j * busSchedule.TimeInterval[i] + busSchedule.TimeRunning[i][m])))
                                    {
                                        model.Add(x[i, j] + busSchedule.TimeRunning[i][m] - x[k, l] - busSchedule.TimeRunning[k][m] - busSchedule.TimeTransfer[m] >= -M * (1 - y[i, j, k, l, m]));
                                        model.Add(x[i, j] + busSchedule.TimeRunning[i][m] - x[k, l] - busSchedule.TimeRunning[k][m] - busSchedule.TimeTransfer[m] <= busSchedule.TimeInterval[i] - 1 + M * (1 - y[i, j, k, l, m]));

                                        model.Add(w[i, j, k, l, m] <= x[i, j] + busSchedule.TimeRunning[i][m] - x[k, l] - busSchedule.TimeRunning[k][m] - busSchedule.TimeTransfer[m] + M * (1 - y[i, j, k, l, m]));
                                        model.Add(w[i, j, k, l, m] >= x[i, j] + busSchedule.TimeRunning[i][m] - x[k, l] - busSchedule.TimeRunning[k][m] - busSchedule.TimeTransfer[m] - M * (1 - y[i, j, k, l, m]));
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
        for (int i = 0; i < busSchedule.TimeInterval.Length; i++)
        {
            for (int j = 0; j < carCount[i] - 1; j++)
            {
                model.Add(x[i, j + 1] - x[i, j] == busSchedule.TimeInterval[i]);
            }
        }

        var objectiveExpr = Google.OrTools.Sat.LinearExpr.Sum(
            from i in Enumerable.Range(0, busSchedule.TimeInterval.Length)
            from k in Enumerable.Range(0, busSchedule.TimeInterval.Length)
            from m in Enumerable.Range(0, busSchedule.TimeRunning[0].Length)
            let sumJl = Google.OrTools.Sat.LinearExpr.Sum(
                from j in Enumerable.Range(0, carCount[i])
                from l in Enumerable.Range(0, carCount[k])
                where i != k
                select y[i, j, k, l, m]
            )
            select sumJl * (int)(transferPassengerFlow[k, i, m])
        );
        model.Add(objectiveExpr == (int)busSchedule.P);

        // Define the objective function (maximization)
        // int scaleFactor = 10;
        var objective = Google.OrTools.Sat.LinearExpr.Sum(
            from i in Enumerable.Range(0, busSchedule.TimeInterval.Length)
            from k in Enumerable.Range(0, busSchedule.TimeInterval.Length)
            from m in Enumerable.Range(0, busSchedule.TimeRunning[0].Length)
            let sumJl = Google.OrTools.Sat.LinearExpr.Sum(
                from j in Enumerable.Range(0, carCount[i])
                from l in Enumerable.Range(0, carCount[k])
                where i != k
                select u[i, j, k, l, m]
            )
            select sumJl * (int)(transferPassengerFlow[k, i, m]) // * scaleFactor)
        );

        model.Minimize(objective);

        // Solve the model
        CpSolver solver = new CpSolver();
        // Set the solver time limit
        solver.StringParameters = $"max_time_in_seconds:{busSchedule.SolverTimeLimit}";
        CpSolverStatus resultStatus = solver.Solve(model);

        // Output the result
        int modelStatus;
        if (resultStatus == CpSolverStatus.Optimal)
        {
            modelStatus = 0;
        }
        else if (resultStatus == CpSolverStatus.Infeasible)
        {
            modelStatus = 2;
        }
        else
        {
            modelStatus = 1;
        }

        // output the result
        int[] firstCar = new int[busSchedule.TimeInterval.Length];
        for (int i = 0; i < busSchedule.TimeInterval.Length; i++)
        {
            firstCar[i] = (int)solver.Value(x[i, 0]);
        }
        OutputBusSchedule outputBusSchedule = new(modelStatus, solver.ObjectiveValue, firstCar);

        return outputBusSchedule;
    }

    private static OutputBusSchedule Optimization_CP_SAT_Model4(BusSchedule busSchedule)
    {
        double[,,] transferPassengerFlow = new double[busSchedule.TimeInterval.Length, busSchedule.TimeInterval.Length, busSchedule.TimeRunning[0].Length];
        // Parse integer
        for (int i = 0; i < busSchedule.TimeInterval.Length; i++)
        {
            for (int j = 0; j < busSchedule.TimeInterval.Length; j++)
            {
                for (int k = 0; k < busSchedule.TimeRunning[0].Length; k++)
                {
                    transferPassengerFlow[i, j, k] = 10 * busSchedule.TransferPassengerFlow[i][j][k];
                }
            }
        }

        // Create the CP-SAT model
        CpModel model = new CpModel();

        int M = 100000;
        int[] carCount = new int[busSchedule.TimeInterval.Length];
        for (int i = 0; i < busSchedule.TimeInterval.Length; i++)
        {
            carCount[i] = (int)Math.Ceiling((double)busSchedule.TotalTimeInterval / (double)busSchedule.TimeInterval[i]); //向上取整
        }

        // Decision variables x[i, j] (integer variables)
        IntVar[,] x = new IntVar[busSchedule.TimeInterval.Length, carCount.Max()];
        for (int i = 0; i < busSchedule.TimeInterval.Length; i++)
        {
            for (int j = 0; j < carCount[i]; j++)
            {
                if (j == 0 && carCount[i] != 1 && (j + 1) * busSchedule.TimeInterval[i] - busSchedule.LastCar[i] > 0)
                {
                    // The first car of each line must start at time 0 and end at time with last car
                    x[i, j] = model.NewIntVar(j * busSchedule.TimeInterval[i], (j + 1) * busSchedule.TimeInterval[i] - busSchedule.LastCar[i], $"x{i}_{j}");
                }
                else if ((j + 1) * busSchedule.TimeInterval[i] >= busSchedule.TotalTimeInterval)
                {
                    x[i, j] = model.NewIntVar(j * busSchedule.TimeInterval[i], busSchedule.TotalTimeInterval, $"x{i}_{j}");
                }
                else
                {
                    x[i, j] = model.NewIntVar(j * busSchedule.TimeInterval[i], (j + 1) * busSchedule.TimeInterval[i], $"x{i}_{j}");
                }
            }
        }

        // Decision variables y[i, j, k, l, m] (binary variables)
        IntVar[,,,,] y = new IntVar[busSchedule.TimeInterval.Length, carCount.Max(), busSchedule.TimeInterval.Length, carCount.Max(), busSchedule.TimeRunning[0].Length];
        IntVar[,,,,] w = new IntVar[busSchedule.TimeInterval.Length, carCount.Max(), busSchedule.TimeInterval.Length, carCount.Max(), busSchedule.TimeRunning[0].Length];
        IntVar W = model.NewIntVar(0, 2 * busSchedule.TotalTimeInterval, $"w");
        for (int i = 0; i < busSchedule.TimeInterval.Length; i++)
        {
            for (int j = 0; j < carCount[i]; j++)
            {
                for (int k = 0; k < busSchedule.TimeInterval.Length; k++)
                {
                    for (int l = 0; l < carCount[k]; l++)
                    {
                        for (int m = 0; m < busSchedule.TimeRunning[0].Length; m++)
                        {
                            y[i, j, k, l, m] = model.NewBoolVar($"y{i}_{j}_{k}_{l}_{m}");
                            w[i, j, k, l, m] = model.NewIntVar(-2 * busSchedule.TotalTimeInterval, 2 * busSchedule.TotalTimeInterval, $"w{i}_{j}_{k}_{l}_{m}");

                            if (i != k)
                            {
                                model.Add(0 >= w[i, j, k, l, m] - W - M * (1 - y[i, j, k, l, m]));
                            }
                        }
                    }
                }
            }
        }

        // Constraints for y variables
        for (int i = 0; i < busSchedule.TimeInterval.Length; i++)
        {
            for (int k = 0; k < busSchedule.TimeInterval.Length; k++)
            {
                for (int l = 0; l < carCount[k]; l++)
                {
                    for (int m = 0; m < busSchedule.TimeRunning[0].Length; m++)
                    {
                        // situation 1: cannot transfer between two lines
                        if (busSchedule.TimeRunning[i][m] == busSchedule.TimeLimit || busSchedule.TimeRunning[k][m] == busSchedule.TimeLimit)
                        {
                            for (int j = 0; j < carCount[i]; j++)
                            {
                                model.Add(y[i, j, k, l, m] == 0);
                            }
                            continue;
                        }

                        model.AddLinearConstraint(Google.OrTools.Sat.LinearExpr.Sum(Enumerable.Range(0, carCount[i]).Select(j => y[i, j, k, l, m])), 0, 1);

                        for (int j = 0; j < carCount[i]; j++)
                        {
                            // situation 2: cannot transfer between two vehicles
                            if (i != k)
                            {
                                if (j == 0)
                                {
                                    if (!((busSchedule.TimeRunning[k][m] + busSchedule.TimeTransfer[m] + l * busSchedule.TimeInterval[k] > (j + 1) * busSchedule.TimeInterval[i] - 1 + busSchedule.TimeRunning[i][m]) || (busSchedule.TimeRunning[k][m] + busSchedule.TimeTransfer[m] + (l + 1) * busSchedule.TimeInterval[k] - 1 + busSchedule.TimeRunning[i][m] - 1 < j * busSchedule.TimeInterval[i] + busSchedule.TimeRunning[i][m])))
                                    {
                                        model.Add(x[i, j] + busSchedule.TimeRunning[i][m] - x[k, l] - busSchedule.TimeRunning[k][m] - busSchedule.TimeTransfer[m] >= -M * (1 - y[i, j, k, l, m]));
                                        model.Add(x[i, j] + busSchedule.TimeRunning[i][m] - x[k, l] - busSchedule.TimeRunning[k][m] - busSchedule.TimeTransfer[m] <= busSchedule.TimeRunning[i][m] + M * (1 - y[i, j, k, l, m]));

                                        model.Add(w[i, j, k, l, m] <= x[i, j] + busSchedule.TimeRunning[i][m] - x[k, l] - busSchedule.TimeRunning[k][m] - busSchedule.TimeTransfer[m] + M * (1 - y[i, j, k, l, m]));
                                        model.Add(w[i, j, k, l, m] >= x[i, j] + busSchedule.TimeRunning[i][m] - x[k, l] - busSchedule.TimeRunning[k][m] - busSchedule.TimeTransfer[m] - M * (1 - y[i, j, k, l, m]));
                                    }
                                    else
                                    {
                                        model.Add(y[i, j, k, l, m] == 0);
                                    }
                                }
                                else
                                {
                                    if (!((busSchedule.TimeRunning[k][m] + busSchedule.TimeTransfer[m] + l * busSchedule.TimeInterval[k] > (j + 1) * busSchedule.TimeInterval[i] - 1 + busSchedule.TimeRunning[i][m]) || (busSchedule.TimeRunning[k][m] + busSchedule.TimeTransfer[m] + (l + 1) * busSchedule.TimeInterval[k] - 1 + busSchedule.TimeInterval[i] - 1 < j * busSchedule.TimeInterval[i] + busSchedule.TimeRunning[i][m])))
                                    {
                                        model.Add(x[i, j] + busSchedule.TimeRunning[i][m] - x[k, l] - busSchedule.TimeRunning[k][m] - busSchedule.TimeTransfer[m] >= -M * (1 - y[i, j, k, l, m]));
                                        model.Add(x[i, j] + busSchedule.TimeRunning[i][m] - x[k, l] - busSchedule.TimeRunning[k][m] - busSchedule.TimeTransfer[m] <= busSchedule.TimeInterval[i] - 1 + M * (1 - y[i, j, k, l, m]));

                                        model.Add(w[i, j, k, l, m] <= x[i, j] + busSchedule.TimeRunning[i][m] - x[k, l] - busSchedule.TimeRunning[k][m] - busSchedule.TimeTransfer[m] + M * (1 - y[i, j, k, l, m]));
                                        model.Add(w[i, j, k, l, m] >= x[i, j] + busSchedule.TimeRunning[i][m] - x[k, l] - busSchedule.TimeRunning[k][m] - busSchedule.TimeTransfer[m] - M * (1 - y[i, j, k, l, m]));
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
        for (int i = 0; i < busSchedule.TimeInterval.Length; i++)
        {
            for (int j = 0; j < carCount[i] - 1; j++)
            {
                model.Add(x[i, j + 1] - x[i, j] == busSchedule.TimeInterval[i]);
            }
        }

        var objectiveExpr = Google.OrTools.Sat.LinearExpr.Sum(
            from i in Enumerable.Range(0, busSchedule.TimeInterval.Length)
            from k in Enumerable.Range(0, busSchedule.TimeInterval.Length)
            from m in Enumerable.Range(0, busSchedule.TimeRunning[0].Length)
            let sumJl = Google.OrTools.Sat.LinearExpr.Sum(
                from j in Enumerable.Range(0, carCount[i])
                from l in Enumerable.Range(0, carCount[k])
                where i != k
                select y[i, j, k, l, m]
            )
            select sumJl * (int)(transferPassengerFlow[k, i, m])
        );
        model.Add(objectiveExpr == (int)busSchedule.P);

        // Define the objective function (maximization)
        // int scaleFactor = 10;
        var objective = W;

        model.Minimize(objective);

        // Solve the model
        CpSolver solver = new CpSolver();
        // 设置求解时间上限
        solver.StringParameters = $"max_time_in_seconds:{busSchedule.SolverTimeLimit}";
        CpSolverStatus resultStatus = solver.Solve(model);

        // Output the result
        int modelStatus;
        if (resultStatus == CpSolverStatus.Optimal)
        {
            modelStatus = 0;
        }
        else if (resultStatus == CpSolverStatus.Infeasible)
        {
            modelStatus = 2;
        }
        else
        {
            modelStatus = 1;
        }

        // output the result
        int[] firstCar = new int[busSchedule.TimeInterval.Length];
        for (int i = 0; i < busSchedule.TimeInterval.Length; i++)
        {
            firstCar[i] = (int)solver.Value(x[i, 0]);
        }
        OutputBusSchedule outputBusSchedule = new(modelStatus, solver.ObjectiveValue, firstCar);

        return outputBusSchedule;
    }
}