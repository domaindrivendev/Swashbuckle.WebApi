﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Swashbuckle.Models
{
    public class ModelSpecRegistrar
    {
        private readonly Dictionary<string, ModelSpec> _registeredSpecs;

        public ModelSpecRegistrar()
        {
            _registeredSpecs = new Dictionary<string, ModelSpec>();
        }

        public void Register(ModelSpec modelSpec)
        {
            if (modelSpec.Id == null || modelSpec.Type != "object")
                throw new InvalidOperationException("Registrar expects complex models only - must have non-null Id and Type = \"object\"");

            _registeredSpecs[modelSpec.Id] = modelSpec;
        }

        public void RegisterMany(IEnumerable<ModelSpec> modelSpecs)
        {
            foreach (var modelSpec in modelSpecs)
            {
                Register(modelSpec);
            }
        }

        internal Dictionary<string, ModelSpec> ToDictionary()
        {
            // Don't expose internal state - return a clone. Still shallow ... oh well!
            return _registeredSpecs
                .ToDictionary(item => item.Key, item => item.Value);
        }
    }
}
