using System;
using System.Collections.Generic;
using System.Text;

namespace ChEngine
{
    public static class Evaluation
    {
        public static double Weighting(TypeOfFigure type)
        {
            return type switch
            {
                TypeOfFigure.EMPTY => 0,
                TypeOfFigure.Rook => 5,
                TypeOfFigure.Knight => 3,
                TypeOfFigure.Bishop => 3,
                TypeOfFigure.Queen => 9,
                TypeOfFigure.King => 50,
                TypeOfFigure.Pawn => 1,
                _ => throw new NotImplementedException(),
            };
        }

        public static double GetEvaluation(IEnumerable<Field> fields)
        {
            // simply count pieces
            double score = 0;

            foreach (var field in fields)
                score += Weighting(field.Figure) * (field.IsWhite ? 1 : -1);

            return score;
        }
    }
}
