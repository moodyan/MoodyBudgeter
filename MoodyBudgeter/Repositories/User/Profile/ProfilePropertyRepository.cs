using Microsoft.EntityFrameworkCore;
using MoodyBudgeter.Data.User;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Models.User.Profile;
using MoodyBudgeter.Utility.Repository;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Repositories.User.Profile
{
    public class ProfilePropertyRepository : Repository<ProfileProperty>
    {
        private readonly UnitOfWork Uow;

        public ProfilePropertyRepository(UnitOfWork uow) : base()
        {
            Uow = uow;
        }

        public async override Task<ProfileProperty> Find(int id)
        {
            var dbRecord = await (from r in Uow.DbContext.ProfilePropertyDefinition
                                  where r.PropertyDefinitionId == id
                                  select r).FirstOrDefaultAsync();

            if (dbRecord == null)
            {
                throw new ArgumentException("There is no ProfileProperty with Id " + id + ".");
            }

            return Translate(dbRecord);
        }

        public override IQueryable<ProfileProperty> GetAll()
        {
            return from r in Uow.DbContext.ProfilePropertyDefinition
                   select new ProfileProperty
                   {
                       ProfilePropertyId = r.PropertyDefinitionId,
                       PropertyCategory = r.PropertyCategory,
                       Name = r.PropertyName,
                       Label = !string.IsNullOrEmpty(r.Label) ? r.Label : r.PropertyName,
                       Description = r.Description,
                       InputMask = r.InputMask,
                       Required = r.Required,
                       Visibility = r.DefaultVisibility == 3 ? ProfilePropertyVisibility.Private : (ProfilePropertyVisibility)r.DefaultVisibility,
                       PcreRegex = r.PcreRegex,
                       JsRegex = r.JsRegex,
                       Unique = r.Unique,
                       Ordinal = r.ViewOrder,
                       DataType = (DataType)r.DataTypeEnum,
                       ListId = r.ListId,
                       DateCreated = r.CreatedOnDate ?? new DateTime(),
                       UpdatedBy = r.LastModifiedByUserId ?? 0,
                       DateUpdated = r.LastModifiedOnDate ?? new DateTime()
                   };
        }

        public async override Task<ProfileProperty> Create(ProfileProperty entity)
        {
            var dbRecord = new ProfilePropertyDefinition
            {
                ModuleDefId = -1,
                Deleted = false,
                DataType = 349, // Text. Update me?
                DefaultValue = "",
                PropertyCategory = entity.PropertyCategory,
                PropertyName = entity.Name,
                Length = 100,
                Required = entity.Required,
                ValidationExpression = "",
                ViewOrder = entity.Ordinal,
                Visible = true,
                CreatedByUserId = entity.UpdatedBy, // Set to updatedby on create, wont be updated again.
                CreatedOnDate = DateTime.UtcNow,
                LastModifiedByUserId = entity.UpdatedBy,
                LastModifiedOnDate = DateTime.UtcNow,
                DefaultVisibility = (int)entity.Visibility,
                ReadOnly = false,
                Description = entity.Description,
                Label = entity.Label,
                PcreRegex = entity.PcreRegex,
                JsRegex = entity.JsRegex,
                Unique = entity.Unique,
                InputMask = entity.InputMask,
                ListId = entity.ListId,
                DataTypeEnum = (int)entity.DataType
            };

            Uow.DbContext.ProfilePropertyDefinition.Add(dbRecord);

            await Uow.SaveChanges();

            return Translate(dbRecord);
        }

        public async override Task<ProfileProperty> Update(ProfileProperty entity)
        {
            // Make sure we have valid data.
            if (entity == null)
            {
                throw new ArgumentNullException("Data cannot be null.");
            }

            // Get the existing record from the database.
            var dbRecord = await (from r in Uow.DbContext.ProfilePropertyDefinition
                                  where r.PropertyDefinitionId == entity.ProfilePropertyId
                                  select r).FirstOrDefaultAsync();

            if (dbRecord == null)
            {
                throw new CallerException("There is no ProfileProperty with Id " + entity.ProfilePropertyId + ".");
            }

            dbRecord.PropertyCategory = entity.PropertyCategory;
            dbRecord.PropertyName = entity.Name;
            dbRecord.Required = entity.Required;
            dbRecord.ViewOrder = entity.Ordinal;
            dbRecord.LastModifiedByUserId = entity.UpdatedBy;
            dbRecord.LastModifiedOnDate = DateTime.UtcNow;
            dbRecord.DefaultVisibility = (int)entity.Visibility;
            dbRecord.Description = entity.Description;
            dbRecord.Label = entity.Label;
            dbRecord.PcreRegex = entity.PcreRegex;
            dbRecord.JsRegex = entity.JsRegex;
            dbRecord.Unique = entity.Unique;
            dbRecord.InputMask = entity.InputMask;
            dbRecord.ListId = entity.ListId;
            dbRecord.DataTypeEnum = (int)entity.DataType;

            await Uow.SaveChanges();
            
            return Translate(dbRecord);
        }

        public async override Task Delete(int id)
        {
            var dbRecord = await (from r in Uow.DbContext.ProfilePropertyDefinition
                                  where r.PropertyDefinitionId == id
                                  select r).FirstOrDefaultAsync();

            if (dbRecord == null)
            {
                throw new CallerException("ProfileProperty does not exist.");
            }

            Uow.DbContext.ProfilePropertyDefinition.Remove(dbRecord);
            await Uow.SaveChanges();
        }

        private ProfileProperty Translate(ProfilePropertyDefinition dbRecord)
        {
            return new ProfileProperty
            {
                ProfilePropertyId = dbRecord.PropertyDefinitionId,
                PropertyCategory = dbRecord.PropertyCategory,
                Name = dbRecord.PropertyName,
                Label = !string.IsNullOrEmpty(dbRecord.Label) ? dbRecord.Label : dbRecord.PropertyName,
                Description = dbRecord.Description,
                InputMask = dbRecord.InputMask,
                Required = dbRecord.Required,
                Visibility = dbRecord.DefaultVisibility == 3 ? ProfilePropertyVisibility.Private : (ProfilePropertyVisibility)dbRecord.DefaultVisibility,
                PcreRegex = dbRecord.PcreRegex,
                JsRegex = dbRecord.JsRegex,
                Unique = dbRecord.Unique,
                Ordinal = dbRecord.ViewOrder,
                DataType = (DataType)dbRecord.DataTypeEnum,
                ListId = dbRecord.ListId,
                DateCreated = dbRecord.CreatedOnDate ?? new DateTime(),
                UpdatedBy = dbRecord.LastModifiedByUserId ?? 0,
                DateUpdated = dbRecord.LastModifiedOnDate ?? new DateTime()
            };
        }
    }
}
