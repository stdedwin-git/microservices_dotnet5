using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using ValidationException= Ordering.Application.Exceptions.ValidationException;

namespace Ordering.Application.Behaviours
{
    public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validator;

        public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validator)
        {
            _validator = validator;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            if (_validator.Any())
            {
                var context = new ValidationContext<TRequest>(request);
                var validationsResult =
                    await Task.WhenAll(_validator.Select(v => v.ValidateAsync(context, cancellationToken)));
                var failures = validationsResult.SelectMany(r => r.Errors).Where(f => f != null).ToList();
                if (failures.Count != 0)
                {
                    throw new ValidationException(failures);
                }
            }

            return await next();
        }
    }
}