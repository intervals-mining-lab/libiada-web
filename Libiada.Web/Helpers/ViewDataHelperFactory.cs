namespace Libiada.Web.Helpers;

using Libiada.Database.Models.Repositories.Catalogs;

using System.Security.Claims;

public class ViewDataHelperFactory(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                          Cache cache,
                          IFullCharacteristicRepository fullCharacteristicRepository,
                          ICongenericCharacteristicRepository congenericCharacteristicRepository,
                          IAccordanceCharacteristicRepository accordanceCharacteristicRepository,
                          IBinaryCharacteristicRepository binaryCharacteristicRepository) : IViewDataHelperFactory

{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory = dbFactory;
    private readonly Cache cache = cache;
    private readonly IFullCharacteristicRepository fullCharacteristicRepository = fullCharacteristicRepository;
    private readonly ICongenericCharacteristicRepository congenericCharacteristicRepository = congenericCharacteristicRepository;
    private readonly IAccordanceCharacteristicRepository accordanceCharacteristicRepository = accordanceCharacteristicRepository;
    private readonly IBinaryCharacteristicRepository binaryCharacteristicRepository = binaryCharacteristicRepository;

    public IViewDataHelper Create(ClaimsPrincipal user)
    {
        return new ViewDataHelper(dbFactory, cache, user, fullCharacteristicRepository, congenericCharacteristicRepository, accordanceCharacteristicRepository, binaryCharacteristicRepository);
    }
}
