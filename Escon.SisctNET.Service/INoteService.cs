﻿using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface INoteService : IServiceBase<Model.Note>
    {
        Model.Note FindByNote(string chave, Model.Log log = null);

        Model.Note FindByNote(long noteId, Model.Log log = null);

        List<Model.Note> FindByNotes(long id, string year, string month, Model.Log log = null);

        List<Model.Note> FindByUf(long companyId, string year, string month, string uf, Model.Log log = null);

        List<Model.Note> FindByCompany(long companyId, Model.Log log = null);
    }
}
