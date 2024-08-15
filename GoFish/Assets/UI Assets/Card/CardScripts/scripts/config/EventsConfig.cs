using System;
using events;
using UnityEngine;
using UnityEngine.Events;

namespace config {
    [Serializable]
    public class EventsConfig {
      
        [SerializeField]
        public UnityEvent<CardHover> OnCardHover;
        
        [SerializeField]
        public UnityEvent<CardUnhover> OnCardUnhover;
        
    }
}
