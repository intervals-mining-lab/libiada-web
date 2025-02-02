namespace Libiada.Web.Helpers;

using Libiada.Database.Models.Repositories.Catalogs;
using Libiada.Database.Models.Repositories.Sequences;
using System.Security.Claims;

public class ViewDataHelperFactory(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                          IResearchObjectsCache cache,
                          IFullCharacteristicRepository fullCharacteristicRepository,
                          ICongenericCharacteristicRepository congenericCharacteristicRepository,
                          IAccordanceCharacteristicRepository accordanceCharacteristicRepository,
                          IBinaryCharacteristicRepository binaryCharacteristicRepository) : IViewDataHelperFactory

{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory = dbFactory;
    private readonly IResearchObjectsCache cache = cache;
    private readonly IFullCharacteristicRepository fullCharacteristicRepository = fullCharacteristicRepository;
    private readonly ICongenericCharacteristicRepository congenericCharacteristicRepository = congenericCharacteristicRepository;
    private readonly IAccordanceCharacteristicRepository accordanceCharacteristicRepository = accordanceCharacteristicRepository;
    private readonly IBinaryCharacteristicRepository binaryCharacteristicRepository = binaryCharacteristicRepository;

    public IViewDataHelper Create(ClaimsPrincipal user)
    {
        return new ViewDataHelper(dbFactory, cache, user, fullCharacteristicRepository, congenericCharacteristicRepository, accordanceCharacteristicRepository, binaryCharacteristicRepository);
    }
}
