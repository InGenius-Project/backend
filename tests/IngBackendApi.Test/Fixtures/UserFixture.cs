namespace IngBackendApi.Test.Fixtures;

using IngBackendApi.Enum;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;

public class UserFixture
{
    public Fixture Fixture { get; }

    public UserFixture()
    {
        Fixture = new Fixture();

        Fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());


        // Customize properties as needed
        Fixture.Customize<User>(c => c
            .With(u => u.Id, Guid.Empty)
            .With(u => u.Email, Fixture.Create<string>())
            .With(u => u.Verified, Fixture.Create<bool>())
            .With(u => u.Role, Fixture.Create<UserRole>())
            .With(u => u.Avatar, Fixture.Create<Image>())
            .With(u => u.Username, Fixture.Create<string>())
            .With(u => u.HashedPassword, Fixture.Create<string>())
            .With(u => u.Tags, [])
            .With(u => u.Areas, [])
            .With(u => u.Resumes, [])
            .With(u => u.Recruitments, [])
            .Without(u => u.EmailVerifications)
        );

        Fixture.Customize<UserDTO>(composer =>
        composer.Without(dto => dto.User)
               .Without(dto => dto.Token)
    );

        Fixture.Customize<UserInfoDTO>(composer =>
            composer.With(dto => dto.Id, Guid.NewGuid())
                   .With(dto => dto.Email, Fixture.Create<string>())
                   .With(dto => dto.Verified, Fixture.Create<bool>())
                   .With(dto => dto.Username, Fixture.Create<string>())
                   .With(dto => dto.Avatar, Fixture.Create<ImageDTO>())
                   .With(dto => dto.Role, Fixture.Create<UserRole>())
                   .With(dto => dto.Areas, [])
                   .Without(dto => dto.Tags)
                   .Without(dto => dto.Resumes)
                   .Without(dto => dto.Recruitments)
        );

        Fixture.Customize<UserInfoPostDTO>(composer =>
            composer.Without(dto => dto.Avatar)
                   .With(dto => dto.Username, Fixture.Create<string>())
                   .With(dto => dto.Areas, Fixture.CreateMany<AreaPostDTO>())
                   .Without(dto => dto.Tags)
        );

        Fixture.Customize<UserSignUpDTO>(composer =>
            composer.With(dto => dto.Email, Fixture.Create<string>())
                   .With(dto => dto.Username, Fixture.Create<string>())
                   .With(dto => dto.Password, Fixture.Create<string>())
                   .With(dto => dto.Role, Fixture.Create<UserRole>())
        );

        Fixture.Customize<UserSignInDTO>(composer =>
            composer.With(dto => dto.Email, Fixture.Create<string>())
                   .With(dto => dto.Password, Fixture.Create<string>())
        );

    }

    // Optional: Add methods to customize specific properties

    public User Create() => Fixture.Create<User>();

}
