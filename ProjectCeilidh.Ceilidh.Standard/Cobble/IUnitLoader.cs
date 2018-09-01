using ProjectCeilidh.Cobble;

namespace ProjectCeilidh.Ceilidh.Standard.Cobble
{
    /// <summary>
    /// Handles loading of units into the main <see cref="CobbleContext"/>
    /// </summary>
    public interface IUnitLoader
    {
        /// <summary>
        /// Register all the units this plugin exports
        /// </summary>
        /// <param name="context">The context to register with</param>
        void RegisterUnits(CobbleContext context);
    }
}
