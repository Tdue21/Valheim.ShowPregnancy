// ****************************************************************************
// * The MIT License(MIT)
// * Copyright © 2022 DoveSoft
// *
// * Permission is hereby granted, free of charge, to any person obtaining a
// * copy of this software and associated documentation files (the “Software”),
// * to deal in the Software without restriction, including without limitation
// * the rights to use, copy, modify, merge, publish, distribute, sublicense,
// * and/or sell copies of the Software, and to permit persons to whom the
// * Software is furnished to do so, subject to the following conditions:
// *
// * The above copyright notice and this permission notice shall be included in
// * all copies or substantial portions of the Software.
// *
// * THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS
// * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// * FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL
// * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// * IN THE SOFTWARE.
// ****************************************************************************

using HarmonyLib;

namespace DoveSoft.Valheim.ShowPregnancy
{
    public static class CharacterExtensions
    {
        public static double GetPregnancyDuration(this Character instance)
        {
            return instance.GetProcreation()?.m_pregnancyDuration ?? 0;
        }

        private static Procreation GetProcreation(this Character instance)
        {
            return Traverse.Create(instance)
                           .Field("m_baseAI").GetValue<BaseAI>()?
                           .GetComponent<Procreation>();
        }

        public static Growup GetGrowup(this Character instance)
        {
            return Traverse.Create(instance)
                .Field("m_baseAI").GetValue<BaseAI>()?
                .GetComponent<Growup>();
        }

        private static ZNetView GetZNetView(this Character instance)
        {
            return Traverse.Create(instance).Field("m_nview").GetValue<ZNetView>();
        }

        private static ZNetView GetZNetView(this Procreation instance)
        {
            return Traverse.Create(instance).Field("m_nview").GetValue<ZNetView>();
        }

        public static bool IsPregnant(this Character instance)
        {
            return instance.GetZNetView().IsValid()
                && instance.GetZNetView().GetZDO().GetLong("pregnant") != 0L;
        }

        public static int GetRequiredLovePoints(this Character instance)
        {
            return instance.GetProcreation()?.m_requiredLovePoints ?? 4;

        }
        public static int GetLovePoints(this Character instance)
        {
            return instance.GetProcreation()?
                      .GetZNetView()?
                      .GetZDO()?
                      .GetInt("lovePoints") ?? 0;
        }

        public static long GetPregnancy(this Character instance)
        {
            return instance.GetZNetView().IsValid()
                       ? instance.GetZNetView().GetZDO().GetLong("pregnant")
                       : 0L;
        }
    }
}