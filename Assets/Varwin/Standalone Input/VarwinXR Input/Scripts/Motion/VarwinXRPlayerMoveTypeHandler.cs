using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Varwin.XR
{
    /// <summary>
    /// Контроллер вариантов передвижения игрока.
    /// </summary>
    public class VarwinXRPlayerMoveTypeHandler : MonoBehaviour
    {
        /// <summary>
        /// Ключ хранения настроек типа.
        /// </summary>
        private const string SettingsKey = "vw.player_move_type";
        
        /// <summary>
        /// Контроллер пережвижений.
        /// </summary>
        public VarwinXRPlayerMoveController MoveController;
        
        /// <summary>
        /// Левая рука.
        /// </summary>
        public VarwinXRController LeftHand;
        
        /// <summary>
        /// Правая рука.
        /// </summary>
        public VarwinXRController RightHand;

        /// <summary>
        /// Типы перемещений.
        /// </summary>
        public VarwinXRPlayerMoveBase[] MoveTypes;

        /// <summary>
        /// Исходный механизм для перемещений.
        /// </summary>
        public VarwinXRPlayerMoveBase FallbackMoveType;

        /// <summary>
        /// Текущий режим 
        /// </summary>
        public VarwinXRPlayerMoveBase CurrentMoveType { get; private set; }

        /// <summary>
        /// Имя типа перемещения.
        /// </summary>
        public string MoveTypeName
        {
            get => GetMoveTypeName();
            set => SetMoveTypeName(value);
        }
        
        /// <summary>
        /// Инициализация контроллера перемещений.
        /// </summary>
        private void Awake()
        {
            foreach (var motion in MoveTypes)
            {
                motion.OnInit(MoveController, LeftHand.InputHandler, RightHand.InputHandler);
                motion.IsEnabled = false;
            }

            SetMoveTypeName(MoveTypeName);   
        }

        /// <summary>
        /// Активировать режим.
        /// </summary>
        /// <param name="value">Имя.</param>
        private void SetMoveTypeName(string value)
        {
            var moveType = GetMoveType(value);
            if (!moveType)
            {
                return;
            }
            
            SetMoveType(moveType);
        }
        
        /// <summary>
        /// Получить имя активного режима.
        /// </summary>
        /// <returns>Имя режима.</returns>
        private string GetMoveTypeName()
        {
            if (CurrentMoveType)
            {
                return CurrentMoveType.LocalizationNameKey;
            }
            
            if (PlayerPrefs.HasKey(SettingsKey))
            {
                var moveTypeName = PlayerPrefs.GetString(SettingsKey);
                MoveTypeName = moveTypeName;
            }
            else
            {
                SetMoveType(FallbackMoveType);
            }

            return CurrentMoveType.LocalizationNameKey;
        }
        
        /// <summary>
        /// Активация режима перемещения.
        /// </summary>
        /// <param name="playerMoveBase">Механизм перемещения.</param>
        private void SetMoveType(VarwinXRPlayerMoveBase playerMoveBase)
        {
            foreach (var motion in MoveTypes)
            {
                motion.IsEnabled = motion == playerMoveBase;
            }

            CurrentMoveType = playerMoveBase;
            PlayerPrefs.SetString(SettingsKey, CurrentMoveType.LocalizationNameKey);
        }
        
        /// <summary>
        /// Получить текущий способ перемещения.
        /// </summary>
        /// <param name="name">Имя.</param>
        /// <returns>Способ перемещения.</returns>
        private VarwinXRPlayerMoveBase GetMoveType(string name)
        {
            foreach (var motion in MoveTypes)
            {
                if (motion.LocalizationNameKey == name)
                {
                    return motion;
                }
            }

            return null;
        }

        /// <summary>
        /// Получить список типов перемещения.
        /// </summary>
        /// <returns>Список типов перемещения.</returns>
        public IEnumerable<string> GetMoveTypes()
        {
            return MoveTypes.Select(a => a.LocalizationNameKey);
        }
    }
}