namespace Varwin
{
    public static class LegacyContextEx
    {
        public static GrabingContext GetLegacyGrabbingContext(this GrabInteractionContext context)
        {
            return new GrabingContext
            {
                GameObject = context.InteractHand,
                HandGameObject = context.InteractHand,
                Hand = context.Hand
            };
        }

        public static UsingContext GetLegacyUsingContext(this UseInteractionContext context)
        {
            return new UsingContext
            {
                GameObject = context.InteractHand,
                HandGameObject = context.InteractHand,
                Hand = context.Hand
            };
        }
    }
}