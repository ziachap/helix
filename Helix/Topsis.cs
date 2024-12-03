namespace Helix;

public class Topsis
{
    private double[][] decisionMatrix;
    private double[] weights;
    private int[] benefitCriteria;

    public Topsis(double[][] decisionMatrix, double[] weights, int[] benefitCriteria)
    {
        this.decisionMatrix = decisionMatrix.Select(a => a.ToArray()).ToArray();
        this.weights = (double[])weights.Clone();
        this.benefitCriteria = (int[])benefitCriteria.Clone();
    }

    public double[] Calculate()
    {
        NormalizeDecisionMatrix();
        ApplyWeights();
        double[] idealPositive = CalculateIdealPositive();
        double[] idealNegative = CalculateIdealNegative();
        return CalculatePerformance(idealPositive, idealNegative);
    }

    private void NormalizeDecisionMatrix()
    {
        for (int col = 0; col < decisionMatrix[0].Length; col++)
        {
            double columnSum = 0;
            for (int row = 0; row < decisionMatrix.Length; row++)
            {
                columnSum += Math.Pow(decisionMatrix[row][col], 2);
            }
            double columnMagnitude = Math.Sqrt(columnSum);

            for (int row = 0; row < decisionMatrix.Length; row++)
            {
                decisionMatrix[row][col] /= columnMagnitude;
            }
        }
    }

    private void ApplyWeights()
    {
        for (int row = 0; row < decisionMatrix.Length; row++)
        {
            for (int col = 0; col < decisionMatrix[row].Length; col++)
            {
                decisionMatrix[row][col] *= weights[col];
            }
        }
    }

    private double[] CalculateIdealPositive()
    {
        double[] idealPositive = new double[decisionMatrix[0].Length];
        for (int col = 0; col < decisionMatrix[0].Length; col++)
        {
            idealPositive[col] = benefitCriteria.Contains(col) ? decisionMatrix.Max(r => r[col]) : decisionMatrix.Min(r => r[col]);
        }
        return idealPositive;
    }

    private double[] CalculateIdealNegative()
    {
        double[] idealNegative = new double[decisionMatrix[0].Length];
        for (int col = 0; col < decisionMatrix[0].Length; col++)
        {
            idealNegative[col] = benefitCriteria.Contains(col) ? decisionMatrix.Min(r => r[col]) : decisionMatrix.Max(r => r[col]);
        }
        return idealNegative;
    }

    private double[] CalculatePerformance(double[] idealPositive, double[] idealNegative)
    {
        double[] performance = new double[decisionMatrix.Length];
        for (int row = 0; row < decisionMatrix.Length; row++)
        {
            double positiveDistance = 0;
            double negativeDistance = 0;
            for (int col = 0; col < decisionMatrix[row].Length; col++)
            {
                positiveDistance += Math.Pow(decisionMatrix[row][col] - idealPositive[col], 2);
                negativeDistance += Math.Pow(decisionMatrix[row][col] - idealNegative[col], 2);
            }
            performance[row] = Math.Sqrt(negativeDistance) / (Math.Sqrt(positiveDistance) + Math.Sqrt(negativeDistance));
        }
        return performance;
    }
}