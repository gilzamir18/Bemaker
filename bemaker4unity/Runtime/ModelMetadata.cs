using System.Collections;
using System.Collections.Generic;
using System;
using bemaker;

namespace bemaker
{
    [Serializable]
    public struct ModelInput
    {
        public string name;
        public int[] shape;
        public SensorType type;
        public float rangeMin;
        public float rangeMax;

        public ModelInput(string name, SensorType type, int[] shape, int stackedObservations, float rangeMin, float rangeMax)
        {
            this.name = name;
            
            this.shape = new int[shape.Length + 1];
            this.shape[0] = stackedObservations;
            for (int i = 1; i < this.shape.Length; i++)
            {
                this.shape[i] = shape[i-1];
            }
            this.type = type;
            this.rangeMin = rangeMin;
            this.rangeMax = rangeMax;
        }
    }

    [Serializable]
    public struct ModelOutput
    {
        public string name;
        public bool isContinuous;
        public int[] shape;

        public float[] rangeMin;
        public float[] rangeMax;


        public ModelOutput(string name, int[] shape, bool isContinuous, float[] rangeMin, float[] rangeMax)
        {
            this.name = name;
            this.shape = shape;
            this.isContinuous = isContinuous;
            this.rangeMin = rangeMin;
            this.rangeMax = rangeMax;
        }
    }

    [Serializable]
    public class ModelMetadata
    {
        public ModelInput[] inputs;
        public ModelOutput[] outputs;
        public ModelInput[] fields;

        public ModelMetadata(int inputCount, int outputCount, int fieldsCount)
        {
            inputs = new ModelInput[inputCount];
            outputs = new ModelOutput[outputCount];
            fields = new ModelInput[fieldsCount];
        }

        public void SetField(int idx, ModelInput f)
        {
            this.fields[idx] = f;
        }

        public ModelInput GetField(int idx)
        {
            return this.fields[idx];
        }

        public void SetInput(int idx, ModelInput i)
        {
            this.inputs[idx] = i;
        }

        public ModelInput GetInput(int idx)
        {
            return this.inputs[idx];
        }

        public void SetOutput(int idx, ModelOutput o)
        {
            this.outputs[idx] = o;
        }

        public ModelOutput GetOutput(int idx)
        {
            return this.outputs[idx];
        }

        public int FieldsCount()
        {
            return fields.Length;
        }

        public int InputCount()
        {
            return inputs.Length;
        }

        public int OutputCount()
        {
            return outputs.Length;
        }
    }
}
