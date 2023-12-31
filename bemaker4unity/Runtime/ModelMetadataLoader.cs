using System.Collections;
using System.Collections.Generic;
using bemaker;
using Newtonsoft.Json;


namespace bemaker
{
    public class ModelMetadataLoader
    {

        ModelMetadata metadata;

        public ModelMetadataLoader(BasicAgent agent)
        {
            List<ModelInput> inputs = new List<ModelInput>();
            List<ModelOutput> outputs = new List<ModelOutput>();
            List<ModelInput> fields = new List<ModelInput>();

            foreach(ISensor s in agent.Sensors)
            {
                if (s.IsInput())
                {
                    inputs.Add(new ModelInput(s.GetKey(), 
                                s.GetSensorType(), s.GetShape(), 
                                s.GetStackedObservations(),
                                s.GetRangeMin(), s.GetRangeMax()));
                }
                else if (s.IsState())
                {
                    fields.Add(new ModelInput(s.GetKey(), 
                                s.GetSensorType(), s.GetShape(), 
                                s.GetStackedObservations(),
                                s.GetRangeMin(), s.GetRangeMax()));
                }
            }

            foreach(Actuator a in agent.Actuators)
            {
                if (a.isOutput)
                {
                    outputs.Add(new ModelOutput(a.actionName, a.Shape, a.IsContinuous, a.RangeMin, a.RangeMax));
                }
            }

            metadata = new ModelMetadata(inputs.Count, outputs.Count, fields.Count);
            for (int i = 0; i < inputs.Count; i++)
            {
                metadata.SetInput(i, inputs[i]);
            }

            for (int i = 0; i < outputs.Count; i++)
            {
                metadata.SetOutput(i, outputs[i]);
            }

            for (int i = 0; i < fields.Count; i++)
            {
                metadata.SetField(i, fields[i]);
            }
        }


        public ModelMetadata Metadata
        {
            get
            {
                return metadata;
            }
        }

        public string toJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(metadata);
        }
    }
}
