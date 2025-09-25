using System.Collections.Generic;
using UnityEngine;
using Varwin;
using Varwin.Public;

namespace Varwin.Types.WoodenNumber_2fcd5a29ce1d4430afaebd1220bde758
{
    [VarwinComponent(English: "Wooden Number")]
    public class WoodenNumber : VarwinObject
    {
        [SerializeField] private List<GameObject> numbers;
        private GameObject _activeNumber;

        [VarwinInspector(English: "Value", Russian: "Значение")]
        [Variable(English: "Value", Russian: "Значение")]
        public int Value
        {
            get => _value; 
            set
            {
                if (value >= 0 && value < numbers.Count)
                {
                    _value = value;

                    _activeNumber.SetActive(false);
                    _activeNumber = numbers[value];
                    _activeNumber.SetActive(true);

                    //_activeNumber.GetComponent<MeshRenderer>().sharedMaterial.color = Color;
                }
            }
        }
        private int _value = 0;

        //[VarwinInspector(English: "Color", Russian: "Цвет")]
        //[Variable(English: "Color", Russian: "Цвет")]
        //[field: SerializeField] public Color Color { get; set; }

        private void Awake()
        {
            _activeNumber = numbers[Value];
        }
    }
}
