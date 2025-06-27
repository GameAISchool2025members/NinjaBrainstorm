// Copyright (c) 2023 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.
using UnityEngine;
using System.Collections.Generic;
using Mediapipe.Tasks.Components.Containers;
using System; // Required for Math.Acos, etc.
using System.Linq; // Required for .Average(), .Select()

namespace Mediapipe.Tasks.Vision.HandLandmarker
{
  /// <summary>
  ///   The hand landmarks result from HandLandmarker, where each vector element represents a single hand detected in the image.
  /// </summary>
  public struct HandLandmarkerResult
  {
    /// <summary>
    ///   Classification of handedness.
    /// </summary>
    public List<Classifications> handedness;
    /// <summary>
    ///   Detected hand landmarks in normalized image coordinates.
    /// </summary>
    public List<NormalizedLandmarks> handLandmarks;
    /// <summary>
    ///   Detected hand landmarks in world coordinates.
    /// </summary>
    public List<Landmarks> handWorldLandmarks;

    // Estos campos ahora son parte de la estructura HandLandmarkerResult
    // y se inicializan en el constructor, reflejando el diseño de MediaPipe Unity Plugin.
    public float straightFingerAngleThreshold;
    public float fingerUpToleranceY;

    private List<GestureRule> hardcodedRules;

    internal HandLandmarkerResult(List<Classifications> handedness,
        List<NormalizedLandmarks> handLandmarks, List<Landmarks> handWorldLandmarks)
    {
      this.handedness = handedness;
      this.handLandmarks = handLandmarks;
      this.handWorldLandmarks = handWorldLandmarks;
      this.straightFingerAngleThreshold = 30.0f; // Valor predeterminado
      this.fingerUpToleranceY = 0.05f; // Valor predeterminado

      // Initialize hardcodedRules here
      hardcodedRules = new List<GestureRule>
        {
            new GestureRule
            {
                name = "defend_select_element",
                condition = new Condition
                {
                    middle_finger_stretched = true,
                    index_finger_stretched = false,
                    pinky_finger_stretched = false,
                    ring_finger_stretched = false,
                    general_hand_angle_check = true,
                    max_general_hand_angle = 20f
                },
                text_to_display = "defend, select element"
            },
            new GestureRule
            {
                name = "attack_select_element",
                condition = new Condition
                {
                    index_finger_stretched = true,
                    pinky_finger_stretched = true,
                    middle_finger_stretched = false,
                    ring_finger_stretched = false,
                    general_hand_angle_check = true,
                    max_general_hand_angle = 20f
                },
                text_to_display = "attack, select element"
            },
            new GestureRule
            {
                name = "fire_power_selected",
                condition = new Condition
                {
                    index_finger_stretched = true,
                    middle_finger_stretched = true,
                    pinky_finger_stretched = false,
                    ring_finger_stretched = false,
                    general_hand_angle_check = true,
                    max_general_hand_angle = 45f
                },
                text_to_display = "fire power selected"
            },
            new GestureRule
            {
                name = "grass_power_selected",
                condition = new Condition
                {
                    index_finger_stretched = true,
                    middle_finger_stretched = true,
                    ring_finger_stretched = true,
                    pinky_finger_stretched = true,
                    fingers_up_check = true,
                    index_finger_up = true,
                    middle_finger_up = true,
                    ring_finger_up = true,
                    pinky_finger_up = true,
                    relative_thumb_angle_check = true,
                    max_relative_thumb_angle = 30f,
                    general_hand_angle_check = true,
                    max_general_hand_angle = 20f
                },
                text_to_display = "grass power selected"
            },
            new GestureRule
            {
                name = "water_power_selected",
                condition = new Condition
                {
                    index_finger_stretched = false,
                    middle_finger_stretched = false,
                    ring_finger_stretched = false,
                    pinky_finger_stretched = false,
                    fingers_up_check = true,
                    index_finger_up = false,
                    middle_finger_up = false,
                    ring_finger_up = false,
                    pinky_finger_up = false,
                    relative_thumb_angle_check = true,
                    max_relative_thumb_angle = 14f,
                    general_hand_angle_check = false // No se verifica el ángulo general para esta regla
                },
                text_to_display = "water power selected"
            }
        };
    }

    public static HandLandmarkerResult Alloc(int capacity)
    {
      var handedness = new List<Classifications>(capacity);
      var handLandmarks = new List<NormalizedLandmarks>(capacity);
      var handWorldLandmarks = new List<Landmarks>(capacity);
      return new HandLandmarkerResult(handedness, handLandmarks, handWorldLandmarks);
    }

    public void CloneTo(ref HandLandmarkerResult destination)
    {
      if (handLandmarks == null)
      {
        destination = default;
        return;
      }

      var dstHandedness = destination.handedness ?? new List<Classifications>(handedness.Count);
      dstHandedness.CopyFrom(handedness);

      var dstHandLandmarks = destination.handLandmarks ?? new List<NormalizedLandmarks>(handLandmarks.Count);
      dstHandLandmarks.CopyFrom(handLandmarks);

      var dstHandWorldLandmarks = destination.handWorldLandmarks ?? new List<Landmarks>(handWorldLandmarks.Count);
      dstHandWorldLandmarks.CopyFrom(handWorldLandmarks);

      destination = new HandLandmarkerResult(dstHandedness, dstHandLandmarks, dstHandWorldLandmarks);
    }

    public class LandmarkCoordinates
    {
      public float x;
      public float y;
      public float z;

      public override string ToString()
      {
        return $"{{ x: {x}, y: {y}, z: {z} }}";
      }
    }

    public override string ToString()
      => $"{{ \"handedness\": {Util.Format(handedness)}, \"handLandmarks\": {Util.Format(handLandmarks)}, \"handWorldLandmarks\": {Util.Format(handWorldLandmarks)} }}";

    // Nota: La clase HandLandmarkData no es necesaria si recibimos directamente List<Landmarks>
    // desde MediaPipe. Mantenerla solo si se deserializa un JSON con esa estructura.
    // Para la integración directa con MediaPipe.Tasks.Components.Containers.Landmarks, no es estrictamente necesaria.
    /*
    public class HandLandmarkData
    {
        public List<Landmark> landmarks;
    }
    */

    // Clase para almacenar el estado de los dedos (ahora completa con todas las propiedades)
    public class FingerStates
    {
      public bool index_finger_stretched;
      public bool middle_finger_stretched;
      public bool ring_finger_stretched;
      public bool pinky_finger_stretched;

      public bool index_finger_up;
      public bool middle_finger_up;
      public bool ring_finger_up;
      public bool pinky_finger_up;

      public float thumb_angle_relative_to_thumb;
      public float index_angle_relative_to_thumb;
      public float middle_angle_relative_to_thumb;
      public float ring_angle_relative_to_thumb;
      public float pinky_angle_relative_to_thumb;

      public float general_hand_angle_to_y_axis;

      public override string ToString()
      {
        string s = "--- DEBUG FINGER STATES ---\n";
        s += $"index_finger_stretched: {index_finger_stretched}\n";
        s += $"middle_finger_stretched: {middle_finger_stretched}\n";
        s += $"ring_finger_stretched: {ring_finger_stretched}\n";
        s += $"pinky_finger_stretched: {pinky_finger_stretched}\n";
        s += $"index_finger_up: {index_finger_up}\n";
        s += $"middle_finger_up: {middle_finger_up}\n";
        s += $"ring_finger_up: {ring_finger_up}\n";
        s += $"pinky_finger_up: {pinky_finger_up}\n";
        s += $"thumb_angle_relative_to_thumb: {thumb_angle_relative_to_thumb:F2}\n";
        s += $"index_angle_relative_to_thumb: {index_angle_relative_to_thumb:F2}\n";
        s += $"middle_angle_relative_to_thumb: {middle_angle_relative_to_thumb:F2}\n";
        s += $"ring_angle_relative_to_thumb: {ring_angle_relative_to_thumb:F2}\n";
        s += $"pinky_angle_relative_to_thumb: {pinky_angle_relative_to_thumb:F2}\n";
        s += $"general_hand_angle_to_y_axis: {general_hand_angle_to_y_axis:F2}\n";
        s += "---------------------------\n";
        return s;
      }
    }

    /// <summary>
    /// Calcula la distancia euclidiana entre dos puntos 2D.
    /// </summary>
    private float CalculateDistance(LandmarkCoordinates p1, LandmarkCoordinates p2)
    {
      return Mathf.Sqrt(Mathf.Pow(p2.x - p1.x, 2) + Mathf.Pow(p2.y - p1.y, 2));
    }

    /// <summary>
    /// Calcula el ángulo en grados entre dos vectores 2D.
    /// Retorna el ángulo interior, entre 0 y 180 grados.
    /// </summary>
    private float CalculateAngleBetweenVectors(float v1_x, float v1_y, float v2_x, float v2_y)
    {
      float dot_product = v1_x * v2_x + v1_y * v2_y;
      float magnitude_v1 = Mathf.Sqrt(v1_x * v1_x + v1_y * v1_y);
      float magnitude_v2 = Mathf.Sqrt(v2_x * v2_x + v2_y * v2_y);

      if (magnitude_v1 == 0 || magnitude_v2 == 0)
      {
        return 0.0f;
      }

      float cosine_angle = dot_product / (magnitude_v1 * magnitude_v2);
      float angle_rad = Mathf.Acos(Mathf.Max(-1.0f, Mathf.Min(1.0f, cosine_angle)));

      return angle_rad * Mathf.Rad2Deg;
    }

    /// <summary>
    /// Calcula el ángulo en grados formado por tres puntos p1-p2-p3,
    /// con p2 como el vértice (la articulación).
    /// El ángulo retornado es el ángulo interior, entre 0 y 180 grados.
    /// </summary>
    private float CalculateJointAngle(LandmarkCoordinates p1, LandmarkCoordinates p2, LandmarkCoordinates p3)
    {
      float vector_p2_p1_x = p1.x - p2.x;
      float vector_p2_p1_y = p1.y - p2.y;

      float vector_p2_p3_x = p3.x - p2.x;
      float vector_p2_p3_y = p3.y - p2.y;

      return CalculateAngleBetweenVectors(vector_p2_p1_x, vector_p2_p1_y, vector_p2_p3_x, vector_p2_p3_y);
    }

    /// <summary>
    /// Comprueba si un dedo está estirado basándose en los ángulos de sus articulaciones.
    /// Un dedo se considera estirado si los ángulos de sus articulaciones son cercanos a 180 grados.
    /// </summary>
    private bool IsFingerStretched(LandmarkCoordinates p_knuckle1, LandmarkCoordinates p_knuckle2, LandmarkCoordinates p_knuckle3, LandmarkCoordinates p_knuckle4, float threshold)
    {
      float angle1 = CalculateJointAngle(p_knuckle1, p_knuckle2, p_knuckle3);
      float angle2 = CalculateJointAngle(p_knuckle2, p_knuckle3, p_knuckle4);

      bool is_straight1 = Mathf.Abs(180 - angle1) < threshold;
      bool is_straight2 = Mathf.Abs(180 - angle2) < threshold;

      return is_straight1 && is_straight2;
    }

    /// <summary>
    /// Comprueba si la punta de un dedo está "más alta" (menor valor de Y en coordenadas de imagen)
    /// que su base, indicando que el dedo apunta hacia arriba.
    /// </summary>
    private bool AreFingersPointingUp(LandmarkCoordinates p_base, LandmarkCoordinates p_tip, float tolerance_y)
    {
      return p_tip.y < (p_base.y - tolerance_y);
    }

    /// <summary>
    /// Procesa los puntos de referencia de una sola mano, determinando si los dedos están estirados
    /// y si apuntan hacia arriba, y calcula ángulos relativos a la muñeca y al pulgar.
    /// También calcula el ángulo general de la mano respecto al eje Y vertical.
    /// </summary>
    /// <param name="handLandmarksCoords">Lista de 21 puntos de referencia de la mano como LandmarkCoordinates.</param>
    /// <param name="debugMode">Si es true, imprime mensajes de depuración en la consola.</param>
    /// <returns>Un objeto FingerStates con el estado de los dedos y los ángulos calculados.</returns>
    public FingerStates ProcessHandLandmarks(List<LandmarkCoordinates> handLandmarksCoords, bool debugMode = false)
    {
      FingerStates fingerStates = new FingerStates();

      if (handLandmarksCoords == null || handLandmarksCoords.Count < 21)
      {
        Debug.LogWarning("Se requieren al menos 21 puntos de referencia para procesar la mano.");
        return fingerStates;
      }

      // Comprobación de estiramiento de dedos
      fingerStates.index_finger_stretched = IsFingerStretched(
          handLandmarksCoords[5], handLandmarksCoords[6], handLandmarksCoords[7], handLandmarksCoords[8], straightFingerAngleThreshold
      );
      fingerStates.middle_finger_stretched = IsFingerStretched(
          handLandmarksCoords[9], handLandmarksCoords[10], handLandmarksCoords[11], handLandmarksCoords[12], straightFingerAngleThreshold
      );
      fingerStates.ring_finger_stretched = IsFingerStretched(
          handLandmarksCoords[13], handLandmarksCoords[14], handLandmarksCoords[15], handLandmarksCoords[16], straightFingerAngleThreshold
      );
      fingerStates.pinky_finger_stretched = IsFingerStretched(
          handLandmarksCoords[17], handLandmarksCoords[18], handLandmarksCoords[19], handLandmarksCoords[20], straightFingerAngleThreshold
      );

      // Comprobación de dirección de dedos (apuntando hacia arriba)
      fingerStates.index_finger_up = AreFingersPointingUp(handLandmarksCoords[5], handLandmarksCoords[8], fingerUpToleranceY);
      fingerStates.middle_finger_up = AreFingersPointingUp(handLandmarksCoords[9], handLandmarksCoords[12], fingerUpToleranceY);
      fingerStates.ring_finger_up = AreFingersPointingUp(handLandmarksCoords[13], handLandmarksCoords[16], fingerUpToleranceY);
      fingerStates.pinky_finger_up = AreFingersPointingUp(handLandmarksCoords[17], handLandmarksCoords[20], fingerUpToleranceY);

      // --- Cálculos de Vectores desde la Muñeca a las Puntas de los Dedos ---
      LandmarkCoordinates wrist = handLandmarksCoords[0]; // Muñeca

      // Definir el vector vertical del eje Y (apuntando hacia arriba)
      // En coordenadas de imagen, un incremento en Y es hacia abajo, así que un vector "hacia arriba"
      // en el sistema de coordenadas de la mano sería (0, -1) o similar.
      float vertical_y_axis_vector_x = 0;
      float vertical_y_axis_vector_y = -1; // Apunta hacia la parte superior de la imagen (más pequeño Y)

      // Vector del pulgar (muñeca a punta del pulgar)
      LandmarkCoordinates thumb_tip = handLandmarksCoords[4];
      float v_thumb_x = thumb_tip.x - wrist.x;
      float v_thumb_y = thumb_tip.y - wrist.y;

      // Ángulo del pulgar respecto a sí mismo (debería ser 0)
      fingerStates.thumb_angle_relative_to_thumb = 0.0f;

      // Ángulos de los otros dedos relativos al vector del pulgar, y al eje Y vertical
      List<float> finger_angles_to_y_axis = new List<float>(); // Para calcular el ángulo general de la mano

      // Índice
      LandmarkCoordinates index_tip = handLandmarksCoords[8];
      float v_index_x = index_tip.x - wrist.x;
      float v_index_y = index_tip.y - wrist.y;
      fingerStates.index_angle_relative_to_thumb = CalculateAngleBetweenVectors(v_thumb_x, v_thumb_y, v_index_x, v_index_y);
      finger_angles_to_y_axis.Add(CalculateAngleBetweenVectors(v_index_x, v_index_y, vertical_y_axis_vector_x, vertical_y_axis_vector_y));

      // Medio
      LandmarkCoordinates middle_tip = handLandmarksCoords[12];
      float v_middle_x = middle_tip.x - wrist.x;
      float v_middle_y = middle_tip.y - wrist.y;
      fingerStates.middle_angle_relative_to_thumb = CalculateAngleBetweenVectors(v_thumb_x, v_thumb_y, v_middle_x, v_middle_y);
      finger_angles_to_y_axis.Add(CalculateAngleBetweenVectors(v_middle_x, v_middle_y, vertical_y_axis_vector_x, vertical_y_axis_vector_y));

      // Anular
      LandmarkCoordinates ring_tip = handLandmarksCoords[16];
      float v_ring_x = ring_tip.x - wrist.x;
      float v_ring_y = ring_tip.y - wrist.y;
      fingerStates.ring_angle_relative_to_thumb = CalculateAngleBetweenVectors(v_thumb_x, v_thumb_y, v_ring_x, v_ring_y);
      finger_angles_to_y_axis.Add(CalculateAngleBetweenVectors(v_ring_x, v_ring_y, vertical_y_axis_vector_x, vertical_y_axis_vector_y));

      // Meñique
      LandmarkCoordinates pinky_tip = handLandmarksCoords[20];
      float v_pinky_x = pinky_tip.x - wrist.x;
      float v_pinky_y = pinky_tip.y - wrist.y;
      fingerStates.pinky_angle_relative_to_thumb = CalculateAngleBetweenVectors(v_thumb_x, v_thumb_y, v_pinky_x, v_pinky_y);
      finger_angles_to_y_axis.Add(CalculateAngleBetweenVectors(v_pinky_x, v_pinky_y, vertical_y_axis_vector_x, vertical_y_axis_vector_y));

      // --- Cálculo del Ángulo General de la Mano ---
      if (finger_angles_to_y_axis.Count > 0)
      {
        fingerStates.general_hand_angle_to_y_axis = finger_angles_to_y_axis.Average();
      }
      else
      {
        fingerStates.general_hand_angle_to_y_axis = 0.0f;
      }

      if (debugMode)
      {
        Debug.Log(fingerStates.ToString());
      }

      return fingerStates;
    }

    public class GestureRule
    {
      public string name;
      public Condition condition;
      public string text_to_display;

      public string code; // Código asociado a la regla, por ejemplo "D", "P", "F", "G", "W"
    }

    public class Condition
    {
      public bool? index_finger_stretched; // Usamos 'bool?' para permitir valores nulos (no especificados)
      public bool? middle_finger_stretched;
      public bool? ring_finger_stretched;
      public bool? pinky_finger_stretched;

      public bool? fingers_up_check; // Indica si se deben verificar las condiciones de 'up'
      public bool? index_finger_up;
      public bool? middle_finger_up;
      public bool? ring_finger_up;
      public bool? pinky_finger_up;

      public bool? relative_thumb_angle_check; // Indica si se debe verificar el ángulo relativo del pulgar
      public float? max_relative_thumb_angle; // 'float?' para valores nulos

      public bool? general_hand_angle_check; // Indica si se debe verificar el ángulo general de la mano
      public float? max_general_hand_angle; // 'float?' para valores nulos
    }

    void Awake()
    {
      // Inicializa las reglas hardcodeadas aquí
      hardcodedRules = new List<GestureRule>
        {
            new GestureRule
            {
                name = "defend_select_element",
                condition = new Condition
                {
                    middle_finger_stretched = true,
                    index_finger_stretched = false,
                    pinky_finger_stretched = false,
                    ring_finger_stretched = false,
                    general_hand_angle_check = true,
                    max_general_hand_angle = 20f
                },
                text_to_display = "defend",
                code = "D"
            },
            new GestureRule
            {
                name = "attack_select_element",
                condition = new Condition
                {
                    index_finger_stretched = true,
                    pinky_finger_stretched = true,
                    middle_finger_stretched = false,
                    ring_finger_stretched = false,
                    general_hand_angle_check = true,
                    max_general_hand_angle = 20f
                },
                text_to_display = "attack",
                code = "P"
            },
            new GestureRule
            {
                name = "fire_power_selected",
                condition = new Condition
                {
                    index_finger_stretched = true,
                    middle_finger_stretched = true,
                    pinky_finger_stretched = false,
                    ring_finger_stretched = false,
                    general_hand_angle_check = true,
                    max_general_hand_angle = 45f
                },
                text_to_display = "fire",
                code = "F"
            },
            new GestureRule
            {
                name = "grass_power_selected",
                condition = new Condition
                {
                    index_finger_stretched = true,
                    middle_finger_stretched = true,
                    ring_finger_stretched = true,
                    pinky_finger_stretched = true,
                    fingers_up_check = true,
                    index_finger_up = true,
                    middle_finger_up = true,
                    ring_finger_up = true,
                    pinky_finger_up = true,
                    relative_thumb_angle_check = true,
                    max_relative_thumb_angle = 30f,
                    general_hand_angle_check = true,
                    max_general_hand_angle = 20f
                },
                text_to_display = "grass",
                code = "G"
            },
            new GestureRule
            {
                name = "water_power_selected",
                condition = new Condition
                {
                    index_finger_stretched = false,
                    middle_finger_stretched = false,
                    ring_finger_stretched = false,
                    pinky_finger_stretched = false,
                    fingers_up_check = true,
                    index_finger_up = false,
                    middle_finger_up = false,
                    ring_finger_up = false,
                    pinky_finger_up = false,
                    relative_thumb_angle_check = true,
                    max_relative_thumb_angle = 14f,
                    general_hand_angle_check = false // No se verifica el ángulo general para esta regla
                },
                text_to_display = "water",
                code = "W"
            }
        };
    }

    public string IdentifyGesture(FingerStates fingerStates)
    {
      if (fingerStates == null)
      {
        Debug.LogWarning("FingerStates es nulo. No se puede identificar el gesto.");
        return "";
      }

      foreach (GestureRule rule in hardcodedRules)
      {
        bool match = true;
        Condition condition = rule.condition;

        // Comprobar condiciones de dedos estirados
        if (condition.index_finger_stretched.HasValue && condition.index_finger_stretched.Value != fingerStates.index_finger_stretched) match = false;
        if (condition.middle_finger_stretched.HasValue && condition.middle_finger_stretched.Value != fingerStates.middle_finger_stretched) match = false;
        if (condition.ring_finger_stretched.HasValue && condition.ring_finger_stretched.Value != fingerStates.ring_finger_stretched) match = false;
        if (condition.pinky_finger_stretched.HasValue && condition.pinky_finger_stretched.Value != fingerStates.pinky_finger_stretched) match = false;

        if (!match) continue; // Si ya no coincide, pasa a la siguiente regla

        // Comprobar si los dedos apuntan hacia arriba (si la regla lo requiere)
        if (condition.fingers_up_check.HasValue && condition.fingers_up_check.Value)
        {
          if (condition.index_finger_up.HasValue && condition.index_finger_up.Value != fingerStates.index_finger_up) match = false;
          if (condition.middle_finger_up.HasValue && condition.middle_finger_up.Value != fingerStates.middle_finger_up) match = false;
          if (condition.ring_finger_up.HasValue && condition.ring_finger_up.Value != fingerStates.ring_finger_up) match = false;
          if (condition.pinky_finger_up.HasValue && condition.pinky_finger_up.Value != fingerStates.pinky_finger_up) match = false;
        }
        if (!match) continue;

        // Comprobar ángulo relativo al pulgar (si la regla lo requiere)
        if (condition.relative_thumb_angle_check.HasValue && condition.relative_thumb_angle_check.Value)
        {
          float maxRelativeAngle = condition.max_relative_thumb_angle ?? 180f; // Usar 180 si no se especifica

          // Solo verificamos el ángulo de los dedos ESTIRADOS si la regla lo requiere
          if (fingerStates.index_finger_stretched && fingerStates.index_angle_relative_to_thumb > maxRelativeAngle) match = false;
          if (fingerStates.middle_finger_stretched && fingerStates.middle_angle_relative_to_thumb > maxRelativeAngle) match = false;
          if (fingerStates.ring_finger_stretched && fingerStates.ring_angle_relative_to_thumb > maxRelativeAngle) match = false;
          if (fingerStates.pinky_finger_stretched && fingerStates.pinky_angle_relative_to_thumb > maxRelativeAngle) match = false;
        }
        if (!match) continue;

        // Comprobar ángulo general de la mano respecto al eje Y (si la regla lo requiere)
        if (condition.general_hand_angle_check.HasValue && condition.general_hand_angle_check.Value)
        {
          float maxGeneralAngle = condition.max_general_hand_angle ?? 180f; // Usar 180 si no se especifica
          if (fingerStates.general_hand_angle_to_y_axis > maxGeneralAngle)
          {
            match = false;
          }
        }
        if (!match) continue;

        // Si todas las condiciones coinciden, hemos encontrado el gesto
        return rule.text_to_display;
      }

      // Si ninguna regla coincide
      return "null";
    }
    
    public string getGesture()
    {
      // Assuming that the first hand is the one we want to process
      var currentHandLandmarksMediapipe = handWorldLandmarks[0];

      List<LandmarkCoordinates> landmarkCoordinates = new List<LandmarkCoordinates>();

      // Convert Mediapipe's Landmark objects to our custom LandmarkCoordinates
      foreach (var landmark in currentHandLandmarksMediapipe.landmarks)
      {
        LandmarkCoordinates landmarkCoord = new LandmarkCoordinates();
        landmarkCoord.x = landmark.x;
        landmarkCoord.y = landmark.y;
        landmarkCoord.z = landmark.z;
        landmarkCoordinates.Add(landmarkCoord);
      }

      // Process the converted landmarks
      FingerStates currentStates = ProcessHandLandmarks(landmarkCoordinates, debugMode: false);

      // Debug.Log($"Gesto para State: {IdentifyGesture(currentStates)}");
      return IdentifyGesture(currentStates);
    }
  }
  
}