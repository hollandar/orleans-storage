using FluentValidation;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using System.Runtime.CompilerServices;

namespace Webefinity.Validation.Components
{
    public class FluentValidator<TModelType, TValidator> : ComponentBase where TValidator : IValidator, new()
    {
        private readonly static char[] separators = new[] { '.', '[' };
        private TValidator validator;
        private bool formValidationRequested = false;

        [CascadingParameter] private EditContext? EditContext { get; set; }

        public FluentValidator() : base()
        {
            validator = new TValidator();
        }

        protected override void OnInitialized()
        {
            if (EditContext == null)
            {
                throw new ArgumentException("EditContext is required, ensure FluidValidator is used within an EditForm", nameof(EditContext));
            }

            var messages = new ValidationMessageStore(EditContext);

            // Revalidate when any field changes, or if the entire form requests validation
            // (e.g., on submit)

            EditContext.OnFieldChanged += (sender, eventArgs) =>
            {
                if (sender is EditContext)
                {
                    if (formValidationRequested)
                        ValidateModel((EditContext)sender, messages);
                }
                else
                {
                    throw new ArgumentException("sender is not an EditContext", nameof(sender));
                }
            };

            EditContext.OnValidationRequested += (sender, eventArgs) =>
            {
                if (sender is EditContext)
                {
                    this.formValidationRequested = true;
                    ValidateModel((EditContext)sender, messages);
                }
                else
                {
                    throw new ArgumentException("sender is not an EditContext", nameof(sender));
                }
            };
        }

        private void ValidateModel(EditContext editContext, ValidationMessageStore messages)
        {
            var validationResult = validator.Validate(new ValidationContext<TModelType>((TModelType)editContext.Model));
            messages.Clear();
            foreach (var error in validationResult.Errors)
            {
                var fieldIdentifier = ToFieldIdentifier(editContext, error.PropertyName);
                messages.Add(fieldIdentifier, error.ErrorMessage);
            }
            editContext.NotifyValidationStateChanged();
        }

        private static FieldIdentifier ToFieldIdentifier(EditContext editContext, string propertyPath)
        {
            // This method parses property paths like 'SomeProp.MyCollection[123].ChildProp'
            // and returns a FieldIdentifier which is an (instance, propName) pair. For example,
            // it would return the pair (SomeProp.MyCollection[123], "ChildProp"). It traverses
            // as far into the propertyPath as it can go until it finds any null instance.

            var obj = editContext.Model;

            while (true)
            {
                var nextTokenEnd = propertyPath.IndexOfAny(separators);
                if (nextTokenEnd < 0)
                {
                    return new FieldIdentifier(obj, propertyPath);
                }

                var nextToken = propertyPath.Substring(0, nextTokenEnd);
                propertyPath = propertyPath.Substring(nextTokenEnd + 1);

                object? newObj = null;
                if (nextToken.EndsWith("]"))
                {
                    // It's an indexer
                    // This code assumes C# conventions (one indexer named Item with one param)
                    nextToken = nextToken.Substring(0, nextToken.Length - 1);
                    if (obj.GetType().GetGenericTypeDefinition() == typeof(HashSet<>))
                    {
                        System.Collections.IEnumerator? enumerable = (System.Collections.IEnumerator?)obj.GetType().GetMethod("GetEnumerator")?.Invoke(obj, null);
                        var count = (int)Convert.ChangeType(nextToken, typeof(int));
                        while (count-- >= 0)
                            newObj = enumerable?.MoveNext() ?? false ? enumerable.Current : null;
                    }
                    else
                    {
                        var prop = obj.GetType().GetProperty("Item");
                        if (prop != null)
                        {
                            var indexerType = prop.GetIndexParameters()[0].ParameterType;
                            var indexerValue = Convert.ChangeType(nextToken, indexerType);
                            newObj = prop.GetValue(obj, new object[] { indexerValue });
                        }
                        else
                        {
                            throw new ArgumentException($"Could not find indexer on object of type {obj.GetType().FullName}.");
                        }
                    }
                }
                else
                {
                    // It's a regular property
                    var prop = obj.GetType().GetProperty(nextToken);
                    if (prop == null)
                    {
                        throw new InvalidOperationException($"Could not find property named {nextToken} on object of type {obj.GetType().FullName}.");
                    }
                    newObj = prop.GetValue(obj);
                }

                if (newObj == null)
                {
                    // This is as far as we can go
                    return new FieldIdentifier(obj, nextToken);
                }

                obj = newObj;
            }
        }
    }
}
